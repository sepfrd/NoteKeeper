using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using NoteKeeper.Business.Constants;
using NoteKeeper.Business.Dtos;
using NoteKeeper.Business.Interfaces;
using NoteKeeper.DataAccess.Entities;
using NoteKeeper.DataAccess.Interfaces;

namespace NoteKeeper.Business.Services.OAuth.V2;

public class GoogleOAuth2Service : IGoogleOAuth2Service
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
    private readonly IRepositoryBase<GoogleToken> _googleTokenRepository;
    private readonly IRepositoryBase<User> _userRepository;
    private readonly IAuthService _authService;

    public GoogleOAuth2Service(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        MemoryCacheEntryOptions memoryCacheEntryOptions,
        IRepositoryBase<GoogleToken> googleTokenRepository,
        IRepositoryBase<User> userRepository,
        IAuthService authService)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _memoryCacheEntryOptions = memoryCacheEntryOptions;
        _googleTokenRepository = googleTokenRepository;
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<ResponseDto<string?>> UseGoogleOAuth2Async(CancellationToken cancellationToken = default)
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
            return GoogleGenerateOAuth2RequestUrl(user);
        }

        if (user.GoogleToken.IsExpired)
        {
            return await GoogleRefreshAccessTokenAsync(user, cancellationToken);
        }

        return new ResponseDto<string?>
        {
            IsSuccess = true,
            Message = MessageConstants.GoogleOAuthSuccessMessage,
            HttpStatusCode = HttpStatusCode.OK
        };
    }

    private ResponseDto<string?> GoogleGenerateOAuth2RequestUrl(User user)
    {
        var googleOAuthConfigurationDto = _configuration
            .GetSection(ConfigurationConstants.GoogleOAuth2Configuration)
            .Get<GoogleOAuth2ConfigurationDto>()!;

        var state = Guid.NewGuid().ToString();

        StoreStateAndUserIdInMemoryCache(state, user.Id);

        var scopes = string.Join(' ', OAuthScopeConstants.EmailScope, OAuthScopeConstants.ProfileScope);

        IEnumerable<KeyValuePair<string, string?>> queryParameters =
        [
            new KeyValuePair<string, string?>(GoogleOAuth2Constants.ScopeParameterName, scopes),
            new KeyValuePair<string, string?>(GoogleOAuth2Constants.AccessTypeParameterName, GoogleOAuth2Constants.OfflineAccessType),
            new KeyValuePair<string, string?>(GoogleOAuth2Constants.IncludeGrantedScopesParameterName, true.ToString().ToLowerInvariant()),
            new KeyValuePair<string, string?>(GoogleOAuth2Constants.ResponseTypeParameterName, GoogleOAuth2Constants.CodeResponseType),
            new KeyValuePair<string, string?>(GoogleOAuth2Constants.StateParameterName, state),
            new KeyValuePair<string, string?>(GoogleOAuth2Constants.RedirectUriParameterName, googleOAuthConfigurationDto.RedirectUri),
            new KeyValuePair<string, string?>(GoogleOAuth2Constants.ClientIdParameterName, googleOAuthConfigurationDto.ClientId),
            new KeyValuePair<string, string?>(GoogleOAuth2Constants.PromptParameterName, GoogleOAuth2Constants.ConsentPrompt)
        ];

        var finalUrl = QueryHelpers.AddQueryString(googleOAuthConfigurationDto.AuthUri, queryParameters);

        return new ResponseDto<string?>
        {
            Data = finalUrl,
            IsSuccess = true,
            HttpStatusCode = HttpStatusCode.OK
        };
    }

    public async Task<ResponseDto<string?>> GoogleExchangeCodeForTokensAsync(
        GoogleExchangeCodeForTokenRequestDto exchangeCodeForTokenRequestDto,
        CancellationToken cancellationToken = default)
    {
        var googleOAuthConfigurationDto = _configuration
            .GetSection(ConfigurationConstants.GoogleOAuth2Configuration)
            .Get<GoogleOAuth2ConfigurationDto>()!;

        var httpClient = _httpClientFactory.CreateClient();

        httpClient.Timeout = TimeSpan.FromSeconds(5d);

        var request = new HttpRequestMessage(HttpMethod.Post, googleOAuthConfigurationDto.TokenUri);

        IEnumerable<KeyValuePair<string, string>> nameValueCollection =
        [
            new KeyValuePair<string, string>(GoogleOAuth2Constants.CodeParameterName, exchangeCodeForTokenRequestDto.Code),
            new KeyValuePair<string, string>(GoogleOAuth2Constants.ClientIdParameterName, googleOAuthConfigurationDto.ClientId),
            new KeyValuePair<string, string>(GoogleOAuth2Constants.ClientSecretParameterName, googleOAuthConfigurationDto.ClientSecret),
            new KeyValuePair<string, string>(GoogleOAuth2Constants.RedirectUriParameterName, googleOAuthConfigurationDto.RedirectUri),
            new KeyValuePair<string, string>(GoogleOAuth2Constants.GrantTypeParameterName, GoogleOAuth2Constants.AuthorizationCodeGrantType)
        ];

        var content = new FormUrlEncodedContent(nameValueCollection);

        request.Content = content;

        var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.GoogleOAuthFailureMessage,
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
                Message = MessageConstants.GoogleOAuthFailureMessage,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }

        var isGoogleTokenStored = await StoreGoogleTokensAsync(
            googleTokenResponseDto,
            exchangeCodeForTokenRequestDto.State,
            cancellationToken);

        if (!isGoogleTokenStored)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.GoogleOAuthFailureMessage,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }

        return new ResponseDto<string?>
        {
            IsSuccess = true,
            Message = MessageConstants.GoogleOAuthSuccessMessage,
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

        var revocationTasks = new List<Task<ResponseDto<string?>>>
        {
            GoogleRevokeTokenAsync(user.GoogleToken.RefreshToken, cancellationToken)
        };

        if (!user.GoogleToken.IsExpired)
        {
            revocationTasks.Add(GoogleRevokeTokenAsync(user.GoogleToken.AccessToken, cancellationToken));
        }

        var revocationResults = await Task.WhenAll(revocationTasks);

        if (revocationResults.Any(result => !result.IsSuccess))
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

    private async Task<ResponseDto<string?>> GoogleRevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var googleOAuthConfigurationDto = _configuration
            .GetSection(ConfigurationConstants.GoogleOAuth2Configuration)
            .Get<GoogleOAuth2ConfigurationDto>()!;

        var httpClient = _httpClientFactory.CreateClient();

        var requestUri = QueryHelpers.AddQueryString(
            googleOAuthConfigurationDto.RevokeUri,
            GoogleOAuth2Constants.TokenParameterName,
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

    private async Task<ResponseDto<string?>> GoogleRefreshAccessTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        var googleOAuthConfigurationDto = _configuration
            .GetSection(ConfigurationConstants.GoogleOAuth2Configuration)
            .Get<GoogleOAuth2ConfigurationDto>()!;

        var httpClient = _httpClientFactory.CreateClient();

        httpClient.Timeout = TimeSpan.FromSeconds(5d);

        var request = new HttpRequestMessage(HttpMethod.Post, googleOAuthConfigurationDto.TokenUri);

        IEnumerable<KeyValuePair<string, string>> nameValueCollection =
        [
            new KeyValuePair<string, string>(GoogleOAuth2Constants.ClientIdParameterName, googleOAuthConfigurationDto.ClientId),
            new KeyValuePair<string, string>(GoogleOAuth2Constants.ClientSecretParameterName, googleOAuthConfigurationDto.ClientSecret),
            new KeyValuePair<string, string>(GoogleOAuth2Constants.GrantTypeParameterName, GoogleOAuth2Constants.RefreshTokenGrantType),
            new KeyValuePair<string, string>(GoogleOAuth2Constants.RefreshTokenParameterName, user.GoogleToken!.RefreshToken)
        ];

        var content = new FormUrlEncodedContent(nameValueCollection);

        request.Content = content;

        var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.GoogleOAuthFailureMessage,
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
                Message = MessageConstants.GoogleOAuthFailureMessage,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }

        user.GoogleToken.Scope = googleTokenResponseDto.Scope;
        user.GoogleToken.AccessToken = googleTokenResponseDto.AccessToken;
        user.GoogleToken.ExpiresAt = DateTime.UtcNow.AddSeconds(googleTokenResponseDto.ExpiresIn);
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
                Message = MessageConstants.GoogleOAuthSuccessMessage,
                HttpStatusCode = HttpStatusCode.OK
            };
        }

        return new ResponseDto<string?>
        {
            IsSuccess = false,
            Message = MessageConstants.GoogleOAuthFailureMessage,
            HttpStatusCode = HttpStatusCode.InternalServerError
        };
    }

    private async Task<bool> StoreGoogleTokensAsync(GoogleTokenResponseDto googleTokenResponseDto, string state, CancellationToken cancellationToken = default)
    {
        var userId = RetrieveUserIdFromMemoryCache(state);

        if (userId is null)
        {
            return false;
        }

        var user = await _userRepository
            .GetByIdAsync(
                userId.Value,
                users => users.Include(user => user.GoogleToken),
                cancellationToken);

        if (user!.GoogleToken is not null)
        {
            user.GoogleToken.AccessToken = googleTokenResponseDto.AccessToken;
            user.GoogleToken.ExpiresAt = DateTime.UtcNow.AddSeconds(googleTokenResponseDto.ExpiresIn);
            user.GoogleToken.RefreshToken = googleTokenResponseDto.RefreshToken!;
            user.GoogleToken.Scope = googleTokenResponseDto.Scope;
            user.GoogleToken.TokenType = googleTokenResponseDto.TokenType;
            user.GoogleToken.IdToken = googleTokenResponseDto.IdToken;
            user.GoogleToken.MarkAsUpdated();
            user.MarkAsUpdated();

            var updateCount = await _googleTokenRepository.SaveChangesAsync(cancellationToken);

            return updateCount > 0;
        }

        var googleToken = googleTokenResponseDto.ToGoogleTokenDomainEntity();

        googleToken.UserId = user.Id;

        await _googleTokenRepository.CreateAsync(googleToken, cancellationToken);

        user.MarkAsUpdated();

        var insertCount = await _googleTokenRepository.SaveChangesAsync(cancellationToken);

        return insertCount > 0;
    }

    private void StoreStateAndUserIdInMemoryCache(string state, long userId) =>
        _memoryCache.Set(state, userId, _memoryCacheEntryOptions);

    private long? RetrieveUserIdFromMemoryCache(string state) =>
        _memoryCache.TryGetValue<long>(state, out var userId) ? userId : null;
}