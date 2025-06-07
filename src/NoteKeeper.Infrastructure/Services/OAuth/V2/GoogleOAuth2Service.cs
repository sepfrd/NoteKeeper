using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Domain.Common;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Infrastructure.Common.Constants;
using NoteKeeper.Infrastructure.Common.Dtos.Configurations;
using NoteKeeper.Infrastructure.Common.Dtos.Google;
using NoteKeeper.Infrastructure.Interfaces;
using NoteKeeper.Shared.Resources;

namespace NoteKeeper.Infrastructure.Services.OAuth.V2;

public class GoogleOAuth2Service : IGoogleOAuth2Service
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly GoogleOAuthOptions _googleOAuthOptions;

    public GoogleOAuth2Service(
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        MemoryCacheEntryOptions memoryCacheEntryOptions,
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ITokenService tokenService,
        IOptions<AppOptions> appOptions)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _memoryCacheEntryOptions = memoryCacheEntryOptions;
        _unitOfWork = unitOfWork;
        _authService = authService;
        _tokenService = tokenService;
        _googleOAuthOptions = appOptions.Value.GoogleOAuthOptions;
    }

    public async Task<DomainResult<string?>> AuthenticateWithGoogleAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetSignedInUserAsync(cancellationToken: cancellationToken);

        if (user is not null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.AlreadySignedIn, StatusCodes.Status403Forbidden);
        }

        return GoogleGenerateOAuth2RequestUrl();
    }

    public async Task<DomainResult<string?>> CompleteGoogleAuthenticationAsync(CompleteGoogleAuthenticationAsyncRequestDto requestDto, CancellationToken cancellationToken = default)
    {
        var username = RetrieveUsernameFromMemoryCache(requestDto.State);

        if (username != CustomOAuthConstants.GoogleSignupPendingUsername)
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
            user = idTokenDto.ToUserDomainEntity();

            user.GoogleToken = googleTokenResponseDto.ToGoogleTokenDomainEntity();

            var createdUser = await _unitOfWork.UserRepository.CreateAsync(user, cancellationToken);

            var insertCount = await _unitOfWork.CommitChangesAsync(cancellationToken);

            if (insertCount < 2)
            {
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

        user.GoogleToken!.AccessToken = googleTokenResponseDto.AccessToken;
        user.GoogleToken.RefreshToken = googleTokenResponseDto.RefreshToken ?? user.GoogleToken.RefreshToken;
        user.GoogleToken.IdToken = googleTokenResponseDto.IdToken;
        user.GoogleToken.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(googleTokenResponseDto.ExpiresIn);
        user.GoogleToken.TokenType = googleTokenResponseDto.TokenType;
        user.GoogleToken.Scope = googleTokenResponseDto.Scope;
        user.GoogleToken.MarkAsUpdated();
        user.MarkAsUpdated();

        var updateCount = await _unitOfWork.CommitChangesAsync(cancellationToken);

        if (updateCount < 2)
        {
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
        var user = await GetSignedInUserAsync(
            [user => user.GoogleToken],
            cancellationToken);

        if (user is null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleTokenRevocation, StatusCodes.Status401Unauthorized);
        }

        if (user.GoogleToken is null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleTokenRevocation, StatusCodes.Status400BadRequest);
        }

        var revocationResult =
            user.GoogleToken.IsExpired
                ? await GoogleRevokeTokenAsync(user.GoogleToken.RefreshToken, cancellationToken)
                : await GoogleRevokeTokenAsync(user.GoogleToken.AccessToken, cancellationToken);

        if (!revocationResult.IsSuccess)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleTokenRevocation, StatusCodes.Status500InternalServerError);
        }

        _unitOfWork.GoogleTokenRepository.Delete(user.GoogleToken);

        user.MarkAsUpdated();

        var updateCount = await _unitOfWork.CommitChangesAsync(cancellationToken);

        if (updateCount > 1)
        {
            return DomainResult<string?>.CreateSuccess(null, StatusCodes.Status200OK, SuccessMessages.GoogleTokenRevocation);
        }

        return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleTokenRevocation, StatusCodes.Status500InternalServerError);
    }

    public async Task<DomainResult<string?>> GoogleRefreshAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetSignedInUserAsync(
            [user => user.GoogleToken],
            cancellationToken);

        if (user is null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleRefreshToken, StatusCodes.Status401Unauthorized);
        }

        if (user.GoogleToken is null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.GoogleRefreshToken, StatusCodes.Status400BadRequest);
        }

        var httpClient = _httpClientFactory.CreateClient();

        httpClient.Timeout = TimeSpan.FromSeconds(5d);

        var request = new HttpRequestMessage(HttpMethod.Post, _googleOAuthOptions.TokenUri);

        IEnumerable<KeyValuePair<string, string>> nameValueCollection =
        [
            new(CustomOAuthConstants.ClientIdParameterName, _googleOAuthOptions.ClientId),
            new(CustomOAuthConstants.ClientSecretParameterName, _googleOAuthOptions.ClientSecret),
            new(CustomOAuthConstants.GrantTypeParameterName, CustomOAuthConstants.RefreshTokenGrantType),
            new(CustomOAuthConstants.RefreshTokenParameterName, user.GoogleToken!.RefreshToken)
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

        user.GoogleToken.Scope = googleTokenResponseDto.Scope;
        user.GoogleToken.AccessToken = googleTokenResponseDto.AccessToken;
        user.GoogleToken.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(googleTokenResponseDto.ExpiresIn);
        user.GoogleToken.IdToken = googleTokenResponseDto.IdToken;
        user.GoogleToken.TokenType = googleTokenResponseDto.TokenType;
        user.GoogleToken.MarkAsUpdated();

        user.MarkAsUpdated();

        var updateCount = await _unitOfWork.CommitChangesAsync(cancellationToken);

        if (updateCount > 0)
        {
            return DomainResult<string?>.CreateSuccess(null, StatusCodes.Status200OK, SuccessMessages.GoogleRefreshToken);
        }

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

        StoreStateAndUsernameInMemoryCache(state, CustomOAuthConstants.GoogleSignupPendingUsername);

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

    private async Task<User?> GetSignedInUserAsync(IEnumerable<Expression<Func<User, object?>>>? includes = null, CancellationToken cancellationToken = default)
    {
        var userUuid = _authService.GetSignedInUserUuid();

        var user = await _unitOfWork.UserRepository.GetOneAsync(
            user => user.Uuid == Guid.Parse(userUuid),
            includes,
            disableTracking: true,
            cancellationToken: cancellationToken);

        return user;
    }

    private void StoreStateAndUsernameInMemoryCache(string state, string username) =>
        _memoryCache.Set(state, username, _memoryCacheEntryOptions);

    private string? RetrieveUsernameFromMemoryCache(string state) =>
        _memoryCache.TryGetValue<string>(state, out var username) ? username : null;
}