using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Domain.Common;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Domain.Enums;
using NoteKeeper.Infrastructure.Common.Constants;
using NoteKeeper.Infrastructure.Common.Dtos.Configurations;
using NoteKeeper.Infrastructure.ExternalServices.Google.Data;
using NoteKeeper.Infrastructure.Interfaces;
using NoteKeeper.Infrastructure.Persistence;
using NoteKeeper.Shared.Constants;
using NoteKeeper.Shared.Resources;

namespace NoteKeeper.Infrastructure.Services.OAuth.V2;

public class GoogleOAuth2Service : IGoogleOAuth2Service
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
    private readonly UnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly IMappingService _mappingService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<GoogleOAuth2Service> _logger;
    private readonly GoogleOAuthOptions _googleOAuthOptions;

    public GoogleOAuth2Service(
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        MemoryCacheEntryOptions memoryCacheEntryOptions,
        UnitOfWork unitOfWork,
        IAuthService authService,
        IMappingService mappingService,
        ITokenService tokenService,
        IOptions<AppOptions> appOptions,
        ILogger<GoogleOAuth2Service> logger)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _memoryCacheEntryOptions = memoryCacheEntryOptions;
        _unitOfWork = unitOfWork;
        _authService = authService;
        _mappingService = mappingService;
        _tokenService = tokenService;
        _logger = logger;
        _googleOAuthOptions = appOptions.Value.GoogleOAuthOptions;
    }

    public async Task<DomainResult<string?>> AuthenticateWithGoogleAsync(CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetSignedInUserAsync(cancellationToken: cancellationToken);

        if (user is not null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.AlreadySignedIn, StatusCodes.Status403Forbidden);
        }

        return GoogleGenerateOAuth2RequestUrl();
    }

    public async Task<DomainResult<string?>> CompleteGoogleAuthenticationAsync(CompleteGoogleAuthenticationAsyncRequestDto requestDto, CancellationToken cancellationToken = default)
    {
        var username = RetrieveUsernameFromMemoryCache(requestDto.State);

        if (username != CustomOAuthConstants.GoogleOidcPendingUsername)
        {
            return DomainResult<string?>.CreateFailure(string.Empty, StatusCodes.Status400BadRequest);
        }

        var googleTokenResponseDto = await GoogleExchangeAuthorizationCodeForTokensAsync(requestDto.Code, cancellationToken);

        if (googleTokenResponseDto?.IdToken is null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleOidc, StatusCodes.Status500InternalServerError);
        }

        var tokenSections = googleTokenResponseDto.IdToken.Split('.');

        if (tokenSections.Length != 3)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleOidc, StatusCodes.Status500InternalServerError);
        }

        var idTokenPayloadString = tokenSections[1];

        var decodedPayloadBytes = Base64UrlEncoder.DecodeBytes(idTokenPayloadString);

        var decodedPayloadString = Encoding.UTF8.GetString(decodedPayloadBytes);

        var idTokenDto = JsonSerializer.Deserialize<GoogleIdTokenPayloadDto>(decodedPayloadString);

        if (idTokenDto is null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleOidc, StatusCodes.Status500InternalServerError);
        }

        var user = await _unitOfWork.UserRepository.GetOneAsync(
            userEntity => userEntity.Username == username,
            disableTracking: true,
            cancellationToken: cancellationToken);

        if (user is null) // Then this is a signup request
        {
            user = _mappingService.Map<GoogleIdTokenPayloadDto, User>(idTokenDto);

            if (user is null)
            {
                _logger.LogCritical(LogMessages.MappingErrorTemplate, typeof(GoogleIdTokenPayloadDto), typeof(User));

                return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleOidc, StatusCodes.Status500InternalServerError);
            }

            var googleToken = _mappingService.Map<GoogleTokenResponseDto, GoogleToken>(googleTokenResponseDto);

            if (googleToken is null)
            {
                _logger.LogCritical(LogMessages.MappingErrorTemplate, typeof(GoogleTokenResponseDto), typeof(GoogleToken));

                return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleOidc, StatusCodes.Status500InternalServerError);
            }

            await _unitOfWork.GoogleTokenRepository.CreateAsync(googleToken, cancellationToken);

            user.ExternalProviderAccounts.Add(new ExternalProviderAccount
            {
                ProviderName = ExternalProviderConstants.Google,
                ProviderType = ProviderType.Oidc,
                LinkedAt = DateTimeOffset.UtcNow,
                UserId = user.Id
            });

            var createdUser = await _unitOfWork.UserRepository.CreateAsync(user, cancellationToken);

            var insertCount = await _unitOfWork.CommitChangesAsync(cancellationToken);

            if (insertCount < 2)
            {
                _logger.LogCritical(LogMessages.DatabasePersistenceErrorTemplate, typeof(GoogleToken), StringConstants.CreateActionName);

                return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleOidc, StatusCodes.Status500InternalServerError);
            }

            var newUserJwt = _tokenService.GenerateEd25519Jwt(createdUser!);

            var signupMessage = string.Format(
                CultureInfo.InvariantCulture,
                SuccessMessages.GoogleSignupTemplate,
                user.FullName);

            return DomainResult<string?>.CreateSuccess(signupMessage, StatusCodes.Status201Created, newUserJwt);
        }

        // This is a signin request

        var existingGoogleToken = await _unitOfWork
            .GoogleTokenRepository
            .GetOneAsync(
                token => token.UserId == user.Id,
                cancellationToken: cancellationToken);

        if (existingGoogleToken is null)
        {
            existingGoogleToken = _mappingService.Map<GoogleTokenResponseDto, GoogleToken>(googleTokenResponseDto);

            if (existingGoogleToken is null)
            {
                _logger.LogCritical(LogMessages.MappingErrorTemplate, typeof(GoogleTokenResponseDto), typeof(GoogleToken));

                return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleOidc, StatusCodes.Status500InternalServerError);
            }
        }

        existingGoogleToken.AccessToken = googleTokenResponseDto.AccessToken;
        existingGoogleToken.RefreshToken = googleTokenResponseDto.RefreshToken ?? existingGoogleToken.RefreshToken;
        existingGoogleToken.IdToken = googleTokenResponseDto.IdToken;
        existingGoogleToken.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(googleTokenResponseDto.ExpiresIn);
        existingGoogleToken.TokenType = googleTokenResponseDto.TokenType;
        existingGoogleToken.Scope = googleTokenResponseDto.Scope;
        existingGoogleToken.MarkAsUpdated();

        if (user.ExternalProviderAccounts.All(account => account.ProviderName != ExternalProviderConstants.Google))
        {
            user.ExternalProviderAccounts.Add(new ExternalProviderAccount
            {
                ProviderName = ExternalProviderConstants.Google,
                ProviderType = ProviderType.Oidc,
                LinkedAt = DateTimeOffset.UtcNow,
                UserId = user.Id
            });
        }

        user.MarkAsUpdated();

        var updateCount = await _unitOfWork.CommitChangesAsync(cancellationToken);

        if (updateCount < 2)
        {
            _logger.LogCritical(LogMessages.DatabasePersistenceErrorTemplate, typeof(GoogleToken), StringConstants.CreateActionName);

            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleOidc, StatusCodes.Status500InternalServerError);
        }

        var existingUserJwt = _tokenService.GenerateEd25519Jwt(user);

        var signinMessage = string.Format(
            CultureInfo.InvariantCulture,
            SuccessMessages.GoogleSigninTemplate,
            user.FullName);

        return DomainResult<string?>.CreateSuccess(signinMessage, StatusCodes.Status200OK, existingUserJwt);
    }

    public async Task<DomainResult<string?>> RevokeTokensAsync(CancellationToken cancellationToken)
    {
        // TODO: use two queries instead of a join
        var user = await _authService.GetSignedInUserAsync(
            [user => user.ExternalProviderAccounts],
            cancellationToken);

        if (user is null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleTokenRevocation, StatusCodes.Status401Unauthorized);
        }

        var account = user
            .ExternalProviderAccounts
            .FirstOrDefault(account => account.ProviderName != ExternalProviderConstants.Google);

        if (account is null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleTokenRevocation, StatusCodes.Status403Forbidden);
        }

        var googleToken = await _unitOfWork
            .GoogleTokenRepository
            .GetOneAsync(
                token => token.UserId == user.Id,
                cancellationToken: cancellationToken);

        if (googleToken is null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleOidc, StatusCodes.Status500InternalServerError);
        }

        var revocationResult =
            googleToken.IsExpired
                ? await GoogleRevokeTokenAsync(googleToken.RefreshToken, cancellationToken)
                : await GoogleRevokeTokenAsync(googleToken.AccessToken, cancellationToken);

        if (!revocationResult.IsSuccess)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleTokenRevocation, StatusCodes.Status500InternalServerError);
        }

        _unitOfWork.GoogleTokenRepository.Delete(googleToken);
        _unitOfWork.ExternalProviderAccountRepository.Delete(account);

        user.MarkAsUpdated();

        var updateCount = await _unitOfWork.CommitChangesAsync(cancellationToken);

        if (updateCount <= 1)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleTokenRevocation, StatusCodes.Status500InternalServerError);
        }

        _logger.LogCritical(LogMessages.DatabasePersistenceErrorTemplate, typeof(GoogleToken), StringConstants.DeleteActionName);

        return DomainResult<string?>.CreateSuccess(null, StatusCodes.Status200OK, SuccessMessages.GoogleTokenRevocation);
    }

    public async Task<DomainResult<string?>> GoogleRefreshAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetSignedInUserAsync(
            [user => user.ExternalProviderAccounts],
            cancellationToken);

        if (user is null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleTokenRevocation, StatusCodes.Status401Unauthorized);
        }

        var account = user
            .ExternalProviderAccounts
            .FirstOrDefault(account => account.ProviderName != ExternalProviderConstants.Google);

        if (account is null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleTokenRevocation, StatusCodes.Status403Forbidden);
        }

        var googleToken = await _unitOfWork
            .GoogleTokenRepository
            .GetOneAsync(
                token => token.UserId == user.Id,
                cancellationToken: cancellationToken);

        if (googleToken is null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleOidc, StatusCodes.Status500InternalServerError);
        }

        var httpClient = _httpClientFactory.CreateClient();

        httpClient.Timeout = TimeSpan.FromSeconds(5d);

        var request = new HttpRequestMessage(HttpMethod.Post, _googleOAuthOptions.TokenUri);

        IEnumerable<KeyValuePair<string, string>> nameValueCollection =
        [
            new(CustomOAuthConstants.ClientIdParameterName, _googleOAuthOptions.ClientId),
            new(CustomOAuthConstants.ClientSecretParameterName, _googleOAuthOptions.ClientSecret),
            new(CustomOAuthConstants.GrantTypeParameterName, CustomOAuthConstants.RefreshTokenGrantType),
            new(CustomOAuthConstants.RefreshTokenParameterName, googleToken.RefreshToken)
        ];

        var content = new FormUrlEncodedContent(nameValueCollection);

        request.Content = content;

        var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleRefreshToken, StatusCodes.Status500InternalServerError);
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        var googleTokenResponseDto = JsonSerializer.Deserialize<GoogleTokenResponseDto>(responseString);

        if (googleTokenResponseDto is null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleRefreshToken, StatusCodes.Status500InternalServerError);
        }

        googleToken.Scope = googleTokenResponseDto.Scope;
        googleToken.AccessToken = googleTokenResponseDto.AccessToken;
        googleToken.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(googleTokenResponseDto.ExpiresIn);
        googleToken.IdToken = googleTokenResponseDto.IdToken;
        googleToken.TokenType = googleTokenResponseDto.TokenType;
        googleToken.MarkAsUpdated();

        account.MarkAsUpdated();
        user.MarkAsUpdated();

        var updateCount = await _unitOfWork.CommitChangesAsync(cancellationToken);

        if (updateCount > 0)
        {
            return DomainResult<string?>.CreateSuccess(null, StatusCodes.Status200OK, SuccessMessages.GoogleRefreshToken);
        }

        _logger.LogCritical(LogMessages.DatabasePersistenceErrorTemplate, typeof(GoogleToken), StringConstants.UpdateActionName);

        return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleRefreshToken, StatusCodes.Status500InternalServerError);
    }

    private async Task<GoogleTokenResponseDto?> GoogleExchangeAuthorizationCodeForTokensAsync(string code, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient();

        httpClient.Timeout = TimeSpan.FromSeconds(5d);

        var request = new HttpRequestMessage(HttpMethod.Post, _googleOAuthOptions.TokenUri);

        IEnumerable<KeyValuePair<string, string>> nameValueCollection =
        [
            new(CustomOAuthConstants.CodeParameterName, code),
            new(CustomOAuthConstants.ClientIdParameterName, _googleOAuthOptions.ClientId),
            new(CustomOAuthConstants.ClientSecretParameterName, _googleOAuthOptions.ClientSecret),
            new(CustomOAuthConstants.RedirectUriParameterName, _googleOAuthOptions.RedirectUri),
            new(CustomOAuthConstants.GrantTypeParameterName, CustomOAuthConstants.AuthorizationCodeGrantType)
        ];

        var content = new FormUrlEncodedContent(nameValueCollection);

        request.Content = content;

        var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        var googleTokenResponseDto = JsonSerializer.Deserialize<GoogleTokenResponseDto>(responseString);

        return googleTokenResponseDto;
    }

    private DomainResult<string?> GoogleGenerateOAuth2RequestUrl()
    {
        var state = Guid.NewGuid().ToString();

        StoreStateAndUsernameInMemoryCache(state, CustomOAuthConstants.GoogleOidcPendingUsername);

        var scopes = string.Join(' ', OAuthScopeConstants.EmailScope, OAuthScopeConstants.ProfileScope, OAuthScopeConstants.OpenIdScope);

        IEnumerable<KeyValuePair<string, string?>> queryParameters =
        [
            new(CustomOAuthConstants.ScopeParameterName, scopes),
            new(CustomOAuthConstants.AccessTypeParameterName, CustomOAuthConstants.OfflineAccessType),
            new(CustomOAuthConstants.IncludeGrantedScopesParameterName, true.ToString().ToLowerInvariant()),
            new(CustomOAuthConstants.ResponseTypeParameterName, CustomOAuthConstants.CodeResponseType),
            new(CustomOAuthConstants.StateParameterName, state),
            new(CustomOAuthConstants.RedirectUriParameterName, _googleOAuthOptions.RedirectUri),
            new(CustomOAuthConstants.ClientIdParameterName, _googleOAuthOptions.ClientId),
            new(CustomOAuthConstants.PromptParameterName, CustomOAuthConstants.ConsentPrompt)
        ];

        var finalUrl = QueryHelpers.AddQueryString(_googleOAuthOptions.AuthUri, queryParameters);

        return DomainResult<string?>.CreateSuccess(null, StatusCodes.Status200OK, finalUrl);
    }

    private async Task<DomainResult<string?>> GoogleRevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var requestUri = QueryHelpers.AddQueryString(
            _googleOAuthOptions.RevokeUri,
            CustomOAuthConstants.TokenParameterName,
            token);

        httpClient.Timeout = TimeSpan.FromSeconds(5d);

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return DomainResult<string?>.CreateSuccess(null, StatusCodes.Status200OK, SuccessMessages.GoogleTokenRevocation);
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        var googleTokenRevocationResponseDto = JsonSerializer.Deserialize<GoogleTokenRevocationResponseDto>(responseString);

        return DomainResult<string?>.CreateFailure(googleTokenRevocationResponseDto!.ErrorDescription ?? string.Empty, (int)response.StatusCode);
    }

    private void StoreStateAndUsernameInMemoryCache(string state, string username) =>
        _memoryCache.Set(state, username, _memoryCacheEntryOptions);

    private string? RetrieveUsernameFromMemoryCache(string state) =>
        _memoryCache.TryGetValue<string>(state, out var username) ? username : null;
}