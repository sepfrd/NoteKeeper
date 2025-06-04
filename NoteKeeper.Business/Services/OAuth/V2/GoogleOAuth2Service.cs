using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NoteKeeper.Business.Constants;
using NoteKeeper.Business.Dtos;
using NoteKeeper.Business.Dtos.Configurations;
using NoteKeeper.Business.Dtos.Google;
using NoteKeeper.Business.Interfaces;
using NoteKeeper.DataAccess.Entities;
using NoteKeeper.DataAccess.Interfaces;

namespace NoteKeeper.Business.Services.OAuth.V2;

public class GoogleOAuth2Service : IGoogleOAuth2Service
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
    private readonly IRepositoryBase<GoogleToken> _googleTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly GoogleOAuthOptions _googleOAuthOptions;

    public GoogleOAuth2Service(
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        MemoryCacheEntryOptions memoryCacheEntryOptions,
        IRepositoryBase<GoogleToken> googleTokenRepository,
        IUserRepository userRepository,
        IAuthService authService,
        ITokenService tokenService,
        IOptions<AppOptions> appOptions)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _memoryCacheEntryOptions = memoryCacheEntryOptions;
        _googleTokenRepository = googleTokenRepository;
        _userRepository = userRepository;
        _authService = authService;
        _tokenService = tokenService;
        _googleOAuthOptions = appOptions.Value.GoogleOAuthOptions;
    }

    public async Task<ResponseDto<string?>> AuthenticateWithGoogleAsync(CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetSignedInUserAsync(null, cancellationToken);

        if (user is not null)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.AlreadySignedInMessage,
                HttpStatusCode = HttpStatusCode.Forbidden
            };
        }

        return GoogleGenerateOAuth2RequestUrl();
    }

    public async Task<ResponseDto<string?>> CompleteGoogleAuthenticationAsync(CompleteGoogleAuthenticationAsyncRequestDto requestDto, CancellationToken cancellationToken = default)
    {
        var username = RetrieveUsernameFromMemoryCache(requestDto.State);

        if (username != CustomOAuthConstants.GoogleSignupPendingUsername)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                HttpStatusCode = HttpStatusCode.BadRequest
            };
        }

        var googleTokenResponseDto = await GoogleExchangeAuthorizationCodeForTokensAsync(requestDto.Code, cancellationToken);

        if (googleTokenResponseDto?.IdToken is null)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.GoogleOidcFailureMessage,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }

        var tokenSections = googleTokenResponseDto.IdToken.Split('.');

        if (tokenSections.Length != 3)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.GoogleOidcFailureMessage,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }

        var idTokenPayloadString = tokenSections[1];

        var decodedPayloadBytes = Base64UrlEncoder.DecodeBytes(idTokenPayloadString);

        var decodedPayloadString = Encoding.UTF8.GetString(decodedPayloadBytes);

        var idTokenDto = JsonSerializer.Deserialize<GoogleIdTokenPayloadDto>(decodedPayloadString);

        if (idTokenDto is null)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.GoogleOidcFailureMessage,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }

        var user = await _userRepository.GetByUsernameAsync(
            idTokenDto.Subject,
            users => users.Include(user => user.GoogleToken),
            cancellationToken);

        if (user is null) // Then this is a signup request
        {
            user = idTokenDto.ToUserDomainEntity();

            user.GoogleToken = googleTokenResponseDto.ToGoogleTokenDomainEntity();

            var createdUser = await _userRepository.CreateAsync(user, cancellationToken);

            var insertCount = await _userRepository.SaveChangesAsync(cancellationToken);

            if (insertCount < 2)
            {
                return new ResponseDto<string?>
                {
                    IsSuccess = false,
                    Message = MessageConstants.GoogleSignupFailureMessage,
                    HttpStatusCode = HttpStatusCode.InternalServerError
                };
            }

            var newUserJwt = _tokenService.GenerateEd25519Jwt(createdUser);

            var signupMessage = string.Format(
                CultureInfo.InvariantCulture,
                MessageConstants.GoogleSignupSuccessMessageTemplate,
                user.FullName);

            return new ResponseDto<string?>
            {
                Data = newUserJwt,
                IsSuccess = true,
                Message = signupMessage,
                HttpStatusCode = HttpStatusCode.Created
            };
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

        var updateCount = await _userRepository.SaveChangesAsync(cancellationToken);

        if (updateCount < 2)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.GoogleSigninFailureMessage,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }

        var existingUserJwt = _tokenService.GenerateEd25519Jwt(user);

        var signinMessage = string.Format(
            CultureInfo.InvariantCulture,
            MessageConstants.GoogleSigninSuccessMessageTemplate,
            user.FullName);

        return new ResponseDto<string?>
        {
            Data = existingUserJwt,
            IsSuccess = true,
            Message = signinMessage,
            HttpStatusCode = HttpStatusCode.OK
        };
    }

    public async Task<ResponseDto<string?>> RevokeTokensAsync(CancellationToken cancellationToken)
    {
        var user = await _authService.GetSignedInUserAsync(
            users => users.Include(user => user.GoogleToken),
            cancellationToken);

        if (user is null)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                HttpStatusCode = HttpStatusCode.Unauthorized
            };
        }

        if (user.GoogleToken is null)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.NoTokensFound,
                HttpStatusCode = HttpStatusCode.BadRequest
            };
        }

        var revocationResult =
            user.GoogleToken.IsExpired
                ? await GoogleRevokeTokenAsync(user.GoogleToken.RefreshToken, cancellationToken)
                : await GoogleRevokeTokenAsync(user.GoogleToken.AccessToken, cancellationToken);

        if (!revocationResult.IsSuccess)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.GoogleTokenRevocationFailureMessage,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }

        _googleTokenRepository.Delete(user.GoogleToken);

        user.MarkAsUpdated();

        var updateCount = await _userRepository.SaveChangesAsync(cancellationToken);

        if (updateCount > 1)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = true,
                Message = MessageConstants.GoogleTokenRevocationSuccessMessage,
                HttpStatusCode = HttpStatusCode.OK
            };
        }

        return new ResponseDto<string?>
        {
            IsSuccess = false,
            Message = MessageConstants.GoogleTokenRevocationFailureMessage,
            HttpStatusCode = HttpStatusCode.InternalServerError
        };
    }

    public async Task<ResponseDto<string?>> GoogleRefreshAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetSignedInUserAsync(
            users => users.Include(user => user.GoogleToken),
            cancellationToken);

        if (user is null)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                HttpStatusCode = HttpStatusCode.Unauthorized
            };
        }

        if (user.GoogleToken is null)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.NoTokensFound,
                HttpStatusCode = HttpStatusCode.BadRequest
            };
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
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.GoogleRefreshTokenFailureMessage,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        var googleTokenResponseDto = JsonSerializer.Deserialize<GoogleTokenResponseDto>(responseString);

        if (googleTokenResponseDto is null)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.GoogleRefreshTokenFailureMessage,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }

        user.GoogleToken.Scope = googleTokenResponseDto.Scope;
        user.GoogleToken.AccessToken = googleTokenResponseDto.AccessToken;
        user.GoogleToken.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(googleTokenResponseDto.ExpiresIn);
        user.GoogleToken.IdToken = googleTokenResponseDto.IdToken;
        user.GoogleToken.TokenType = googleTokenResponseDto.TokenType;
        user.GoogleToken.MarkAsUpdated();

        user.MarkAsUpdated();

        var updateCount = await _userRepository.SaveChangesAsync(cancellationToken);

        if (updateCount > 0)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = true,
                Message = MessageConstants.GoogleRefreshTokenSuccessMessage,
                HttpStatusCode = HttpStatusCode.OK
            };
        }

        return new ResponseDto<string?>
        {
            IsSuccess = false,
            Message = MessageConstants.GoogleRefreshTokenFailureMessage,
            HttpStatusCode = HttpStatusCode.InternalServerError
        };
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

    private ResponseDto<string?> GoogleGenerateOAuth2RequestUrl()
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

        return new ResponseDto<string?>
        {
            Data = finalUrl,
            IsSuccess = true,
            HttpStatusCode = HttpStatusCode.OK
        };
    }

    private async Task<ResponseDto<string?>> GoogleRevokeTokenAsync(string token, CancellationToken cancellationToken = default)
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
            return new ResponseDto<string?>
            {
                IsSuccess = true,
                Message = MessageConstants.GoogleTokenRevocationSuccessMessage,
                HttpStatusCode = HttpStatusCode.OK
            };
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        var googleTokenRevocationResponseDto = JsonSerializer.Deserialize<GoogleTokenRevocationResponseDto>(responseString);

        return new ResponseDto<string?>
        {
            IsSuccess = false,
            Message = googleTokenRevocationResponseDto!.ErrorDescription,
            HttpStatusCode = response.StatusCode
        };
    }

    private void StoreStateAndUsernameInMemoryCache(string state, string username) =>
        _memoryCache.Set(state, username, _memoryCacheEntryOptions);

    private string? RetrieveUsernameFromMemoryCache(string state) =>
        _memoryCache.TryGetValue<string>(state, out var username) ? username : null;
}