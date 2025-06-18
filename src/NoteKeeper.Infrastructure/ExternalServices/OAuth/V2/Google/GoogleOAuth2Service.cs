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
using NoteKeeper.Infrastructure.Common.Dtos.Responses;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google.Models;
using NoteKeeper.Infrastructure.Interfaces;
using NoteKeeper.Infrastructure.Persistence;
using NoteKeeper.Shared.Constants;
using NoteKeeper.Shared.Resources;

namespace NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google;

public class GoogleOAuth2Service : IGoogleOAuth2Service
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
    private readonly UnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly IMappingService _mappingService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<GoogleOAuth2Service> _logger;
    private readonly AppOptions _appOptions;

    public GoogleOAuth2Service(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        MemoryCacheEntryOptions memoryCacheEntryOptions,
        UnitOfWork unitOfWork,
        IAuthService authService,
        IMappingService mappingService,
        ITokenService tokenService,
        IOptions<AppOptions> appOptions,
        ILogger<GoogleOAuth2Service> logger)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _memoryCacheEntryOptions = memoryCacheEntryOptions;
        _unitOfWork = unitOfWork;
        _authService = authService;
        _mappingService = mappingService;
        _tokenService = tokenService;
        _logger = logger;
        _appOptions = appOptions.Value;
    }

    public async Task<DomainResult<string?>> AuthenticateWithGoogleAsync(
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetSignedInUserAsync(cancellationToken: cancellationToken);

        if (user is not null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.AlreadySignedIn, StatusCodes.Status403Forbidden);
        }

        return GoogleGenerateOAuth2RequestUrl(redirectUri);
    }

    public async Task<DomainResult<CompleteGoogleAuthenticationResponseDto?>> CompleteGoogleAuthenticationAsync(
        CompleteGoogleAuthenticationRequestDto requestDto,
        CancellationToken cancellationToken = default)
    {
        var stateDto = RetrieveStateFromMemoryCache(requestDto.State);

        if (stateDto?.Username != CustomOAuthConstants.GoogleOidcPendingUsername)
        {
            return DomainResult<CompleteGoogleAuthenticationResponseDto?>.CreateFailure(
                string.Empty,
                StatusCodes.Status400BadRequest);
        }

        var googleTokenResponseDto = await GoogleExchangeAuthorizationCodeForTokensAsync(requestDto.Code, cancellationToken);

        if (googleTokenResponseDto?.IdToken is null)
        {
            return DomainResult<CompleteGoogleAuthenticationResponseDto?>.CreateFailure(
                ErrorMessages.GoogleOidc,
                StatusCodes.Status500InternalServerError);
        }

        var tokenSections = googleTokenResponseDto.IdToken.Split('.');

        if (tokenSections.Length != 3)
        {
            return DomainResult<CompleteGoogleAuthenticationResponseDto?>.CreateFailure(
                ErrorMessages.GoogleOidc,
                StatusCodes.Status500InternalServerError);
        }

        var idTokenPayloadString = tokenSections[1];

        var decodedPayloadBytes = Base64UrlEncoder.DecodeBytes(idTokenPayloadString);

        var decodedPayloadString = Encoding.UTF8.GetString(decodedPayloadBytes);

        var idTokenDto = JsonSerializer.Deserialize<GoogleIdTokenPayloadDto>(decodedPayloadString);

        if (idTokenDto is null)
        {
            return DomainResult<CompleteGoogleAuthenticationResponseDto?>.CreateFailure(
                ErrorMessages.GoogleOidc,
                StatusCodes.Status500InternalServerError);
        }

        var user = await _unitOfWork.UserRepository.GetOneAsync(
            userEntity => userEntity.Username == idTokenDto.Subject,
            disableTracking: true,
            cancellationToken: cancellationToken);

        await using var transaction = await _unitOfWork.Database.BeginTransactionAsync(cancellationToken);

        var commitSucceeded = false;

        try
        {
            if (user is null) // Then this is a signup request
            {
                user = _mappingService.Map<GoogleIdTokenPayloadDto, User>(idTokenDto);

                if (user is null)
                {
                    _logger.LogCritical(LogMessages.MappingErrorTemplate, typeof(GoogleIdTokenPayloadDto), typeof(User));

                    return DomainResult<CompleteGoogleAuthenticationResponseDto?>.CreateFailure(
                        ErrorMessages.GoogleOidc,
                        StatusCodes.Status500InternalServerError);
                }

                user.ExternalProviderAccounts.Add(new ExternalProviderAccount
                {
                    ProviderName = ExternalProviderConstants.Google,
                    ProviderType = ProviderType.Oidc,
                    LinkedAt = DateTimeOffset.UtcNow,
                    UserId = user.Id
                });

                var createdUser = await _unitOfWork.UserRepository.CreateAsync(user, cancellationToken);

                await _unitOfWork.CommitChangesAsync(cancellationToken);

                var googleToken = _mappingService.Map<GoogleTokenResponseDto, GoogleToken>(googleTokenResponseDto);

                if (googleToken is null)
                {
                    _logger.LogCritical(LogMessages.MappingErrorTemplate, typeof(GoogleTokenResponseDto), typeof(GoogleToken));

                    return DomainResult<CompleteGoogleAuthenticationResponseDto?>.CreateFailure(
                        ErrorMessages.GoogleOidc,
                        StatusCodes.Status500InternalServerError);
                }

                googleToken.UserId = createdUser!.Id;

                await _unitOfWork.GoogleTokenRepository.CreateAsync(googleToken, cancellationToken);

                await _unitOfWork.CommitChangesAsync(cancellationToken);

                var newUserJwt = _tokenService.GenerateEd25519Jwt(createdUser);
                var signupRefreshToken = await _tokenService.GenerateNewRefreshTokenAsync(createdUser.Id.ToString());

                var signupResponseDto = new CompleteGoogleAuthenticationResponseDto(
                    new AuthResponseDto(
                        newUserJwt,
                        signupRefreshToken,
                        DateTimeOffset.UtcNow.AddMinutes(_appOptions.JwtOptions.RefreshTokenLifeTimeInMinutes)),
                    stateDto.RedirectUri);

                var signupMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    SuccessMessages.GoogleSignupTemplate,
                    user.FullName);

                await transaction.CommitAsync(cancellationToken);

                commitSucceeded = true;

                return DomainResult<CompleteGoogleAuthenticationResponseDto?>.CreateSuccess(
                    signupMessage,
                    StatusCodes.Status308PermanentRedirect,
                    signupResponseDto);
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

                    return DomainResult<CompleteGoogleAuthenticationResponseDto?>.CreateFailure(
                        ErrorMessages.GoogleOidc,
                        StatusCodes.Status500InternalServerError);
                }
            }

            existingGoogleToken.AccessToken = googleTokenResponseDto.AccessToken;
            existingGoogleToken.RefreshToken = googleTokenResponseDto.RefreshToken ?? existingGoogleToken.RefreshToken;
            existingGoogleToken.IdToken = googleTokenResponseDto.IdToken;
            existingGoogleToken.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(googleTokenResponseDto.ExpiresIn);
            existingGoogleToken.TokenType = googleTokenResponseDto.TokenType;
            existingGoogleToken.Scope = googleTokenResponseDto.Scope;

            _unitOfWork.GoogleTokenRepository.Update(existingGoogleToken);

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

            _unitOfWork.UserRepository.Update(user);

            await _unitOfWork.CommitChangesAsync(cancellationToken);

            var existingUserJwt = _tokenService.GenerateEd25519Jwt(user);
            var signinRefreshToken = await _tokenService.GenerateNewRefreshTokenAsync(user.Id.ToString());

            var signinResponseDto = new CompleteGoogleAuthenticationResponseDto(
                new AuthResponseDto(
                    existingUserJwt,
                    signinRefreshToken,
                    DateTimeOffset.UtcNow.AddMinutes(_appOptions.JwtOptions.RefreshTokenLifeTimeInMinutes)),
                stateDto.RedirectUri);

            var signinMessage = string.Format(
                CultureInfo.InvariantCulture,
                SuccessMessages.GoogleSigninTemplate,
                user.FullName);

            await transaction.CommitAsync(cancellationToken);

            commitSucceeded = true;

            return DomainResult<CompleteGoogleAuthenticationResponseDto?>.CreateSuccess(
                signinMessage,
                StatusCodes.Status308PermanentRedirect,
                signinResponseDto);
        }
        catch (Exception exception)
        {
            _logger.LogError(LogMessages.DatabasePersistenceErrorTemplate, typeof(GoogleToken), StringConstants.CreateActionName);
            _logger.LogError(exception, LogMessages.ExceptionTemplate, exception.InnerException);

            await transaction.RollbackAsync(cancellationToken);

            return DomainResult<CompleteGoogleAuthenticationResponseDto?>.CreateFailure(
                ErrorMessages.GoogleOidc,
                StatusCodes.Status500InternalServerError);
        }
        finally
        {
            if (!commitSucceeded)
            {
                await transaction.RollbackAsync(cancellationToken);
            }
        }
    }

    public async Task<DomainResult<string?>> RevokeTokensAsync(CancellationToken cancellationToken)
    {
        var user = await _authService.GetSignedInUserAsync(cancellationToken: cancellationToken);

        if (user is null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleTokenRevocation, StatusCodes.Status401Unauthorized);
        }

        var account = await _unitOfWork
            .ExternalProviderAccountRepository
            .GetOneAsync(account =>
                    account.UserId == user.Id &&
                    account.ProviderName == ExternalProviderConstants.Google,
                cancellationToken: cancellationToken);

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

        _unitOfWork.UserRepository.Update(user);

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

        var request = new HttpRequestMessage(HttpMethod.Post, _appOptions.GoogleOAuthOptions.TokenUri);

        IEnumerable<KeyValuePair<string, string>> nameValueCollection =
        [
            new(CustomOAuthConstants.ClientIdParameterName, _appOptions.GoogleOAuthOptions.ClientId),
            new(CustomOAuthConstants.ClientSecretParameterName, _appOptions.GoogleOAuthOptions.ClientSecret),
            new(CustomOAuthConstants.GrantTypeParameterName, CustomOAuthConstants.RefreshTokenGrantType),
            new(CustomOAuthConstants.RefreshTokenParameterName, googleToken.RefreshToken)
        ];

        var content = new FormUrlEncodedContent(nameValueCollection);

        request.Content = content;

        var response = await _httpClient.SendAsync(request, cancellationToken);

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

        _unitOfWork.GoogleTokenRepository.Update(googleToken);
        _unitOfWork.ExternalProviderAccounts.Update(account);
        _unitOfWork.UserRepository.Update(user);

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
        var request = new HttpRequestMessage(HttpMethod.Post, _appOptions.GoogleOAuthOptions.TokenUri);

        IEnumerable<KeyValuePair<string, string>> nameValueCollection =
        [
            new(CustomOAuthConstants.CodeParameterName, code),
            new(CustomOAuthConstants.ClientIdParameterName, _appOptions.GoogleOAuthOptions.ClientId),
            new(CustomOAuthConstants.ClientSecretParameterName, _appOptions.GoogleOAuthOptions.ClientSecret),
            new(CustomOAuthConstants.RedirectUriParameterName, _appOptions.GoogleOAuthOptions.RedirectUri),
            new(CustomOAuthConstants.GrantTypeParameterName, CustomOAuthConstants.AuthorizationCodeGrantType)
        ];

        var content = new FormUrlEncodedContent(nameValueCollection);

        request.Content = content;

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        var googleTokenResponseDto = JsonSerializer.Deserialize<GoogleTokenResponseDto>(responseString);

        return googleTokenResponseDto;
    }

    private DomainResult<string?> GoogleGenerateOAuth2RequestUrl(string redirectUri)
    {
        var state = Guid.NewGuid().ToString();

        var stateDto = new StoredStateDto(CustomOAuthConstants.GoogleOidcPendingUsername, redirectUri);

        StoreStateInMemoryCache(state, stateDto);

        var scopes = string.Join(' ', OAuthScopeConstants.EmailScope, OAuthScopeConstants.ProfileScope, OAuthScopeConstants.OpenIdScope);

        IEnumerable<KeyValuePair<string, string?>> queryParameters =
        [
            new(CustomOAuthConstants.ScopeParameterName, scopes),
            new(CustomOAuthConstants.AccessTypeParameterName, CustomOAuthConstants.OfflineAccessType),
            new(CustomOAuthConstants.IncludeGrantedScopesParameterName, true.ToString().ToLowerInvariant()),
            new(CustomOAuthConstants.ResponseTypeParameterName, CustomOAuthConstants.CodeResponseType),
            new(CustomOAuthConstants.StateParameterName, state),
            new(CustomOAuthConstants.RedirectUriParameterName, _appOptions.GoogleOAuthOptions.RedirectUri),
            new(CustomOAuthConstants.ClientIdParameterName, _appOptions.GoogleOAuthOptions.ClientId),
            new(CustomOAuthConstants.PromptParameterName, CustomOAuthConstants.ConsentPrompt)
        ];

        var finalUrl = QueryHelpers.AddQueryString(_appOptions.GoogleOAuthOptions.AuthUri, queryParameters);

        return DomainResult<string?>.CreateSuccess(null, StatusCodes.Status200OK, finalUrl);
    }

    private async Task<DomainResult<string?>> GoogleRevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var requestUri = QueryHelpers.AddQueryString(
            _appOptions.GoogleOAuthOptions.RevokeUri,
            CustomOAuthConstants.TokenParameterName,
            token);

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return DomainResult<string?>.CreateSuccess(null, StatusCodes.Status200OK, SuccessMessages.GoogleTokenRevocation);
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        var googleTokenRevocationResponseDto = JsonSerializer.Deserialize<GoogleTokenRevocationResponseDto>(responseString);

        return DomainResult<string?>.CreateFailure(googleTokenRevocationResponseDto!.ErrorDescription ?? string.Empty, (int)response.StatusCode);
    }

    private void StoreStateInMemoryCache(string state, StoredStateDto stateDto) =>
        _memoryCache.Set(state, stateDto, _memoryCacheEntryOptions);

    private StoredStateDto? RetrieveStateFromMemoryCache(string state) =>
        _memoryCache.TryGetValue<StoredStateDto>(state, out var stateDto) ? stateDto : null;
}