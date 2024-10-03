using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NoteKeeper.Business.Constants;
using NoteKeeper.Business.Dtos;
using NoteKeeper.Business.Interfaces;
using NoteKeeper.Business.Utilities;
using NoteKeeper.DataAccess.Entities;
using NoteKeeper.DataAccess.Interfaces;
using Org.BouncyCastle.Crypto.Signers;

namespace NoteKeeper.Business.Services;

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IRepositoryBase<User> _userRepository;
    private readonly IRepositoryBase<GoogleToken> _googleTokenRepository;

    public AuthService(
        IRepositoryBase<User> userRepository,
        IRepositoryBase<GoogleToken> googleTokenRepository,
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        MemoryCacheEntryOptions memoryCacheEntryOptions,
        IConfiguration configuration,
        JsonSerializerOptions jsonSerializerOptions)
    {
        _userRepository = userRepository;
        _googleTokenRepository = googleTokenRepository;
        _httpContextAccessor = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _memoryCacheEntryOptions = memoryCacheEntryOptions;
        _configuration = configuration;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public async Task<ResponseDto<string?>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        if (IsSignedIn())
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.AlreadySignedInMessage,
                HttpStatusCode = HttpStatusCode.BadRequest
            };
        }

        var user = await ValidateAndGetUserByCredentialsAsync(loginDto, cancellationToken);

        if (user is null)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.InvalidCredentialsMessage,
                HttpStatusCode = HttpStatusCode.BadRequest
            };
        }

        var jwt = GenerateEd25519Jwt(user);

        return new ResponseDto<string?>
        {
            Data = jwt,
            IsSuccess = true,
            Message = MessageConstants.SuccessfulLoginMessage,
            HttpStatusCode = HttpStatusCode.OK
        };
    }

    public bool IsEd25519JwtValid(string token)
    {
        var tokenSections = token.Split('.');

        if (tokenSections.Length != 3)
        {
            return false;
        }

        var header = tokenSections[0];
        var payload = tokenSections[1];
        var signature = tokenSections[2];

        var message = header + '.' + payload;

        var messageBytes = Encoding.UTF8.GetBytes(message);

        var signatureBytes = Base64UrlEncoder.DecodeBytes(signature);

        var jwtConfigurationDto = _configuration.GetSection(ConfigurationConstants.Ed25519JwtConfigurationSectionKey).Get<JwtConfigurationDto>()!;

        var publicKey = CryptographyHelper.LoadPublicKeyFromString(jwtConfigurationDto.PublicKey);

        var verifier = new Ed25519Signer();

        verifier.Init(false, publicKey);
        verifier.BlockUpdate(messageBytes, 0, messageBytes.Length);

        var isValid = verifier.VerifySignature(signatureBytes);

        return isValid;
    }

    public ClaimsPrincipal ConvertJwtStringToClaimsPrincipal(string jwtString, string authenticationType)
    {
        var handler = new JwtSecurityTokenHandler();

        var jwt = handler.ReadJwtToken(jwtString);

        var claims = jwt.Claims;

        var identity = new ClaimsIdentity(claims, authenticationType);

        return new ClaimsPrincipal(identity);
    }

    public async Task<User?> GetSignedInUserAsync(
        Func<IQueryable<User>, IIncludableQueryable<User, object?>>? include = null,
        CancellationToken cancellationToken = default)
    {
        var userUuid = _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtExtendedConstants.JwtUuidClaimType)!;

        var user = await _userRepository.GetByUuidAsync(Guid.Parse(userUuid), include, cancellationToken);

        return user;
    }

    public async Task<ResponseDto<string?>> BuildGoogleOAuth2RequestUrlAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetSignedInUserAsync(null, cancellationToken);

        if (user is null)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                HttpStatusCode = HttpStatusCode.Unauthorized
            };
        }

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
            new KeyValuePair<string, string?>(GoogleOAuth2Constants.ClientIdParameterName, googleOAuthConfigurationDto.ClientId)
        ];

        var finalUrl = QueryHelpers.AddQueryString(googleOAuthConfigurationDto.AuthUri, queryParameters);

        return new ResponseDto<string?>
        {
            Data = finalUrl,
            IsSuccess = true,
            HttpStatusCode = HttpStatusCode.OK
        };
    }

    public async Task<ResponseDto<string?>> GoogleExchangeCodeForTokenAsync(
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
            new KeyValuePair<string, string>(GoogleOAuth2Constants.GrantTypeParameterName, GoogleOAuth2Constants.AuthorizationCodeGrantType),
        ];

        var content = new FormUrlEncodedContent(nameValueCollection);

        request.Content = content;

        var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.UnsuccessfulGoogleTokenRetrievalMessage,
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
                Message = MessageConstants.UnsuccessfulGoogleTokenRetrievalMessage,
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
                Message = MessageConstants.UnsuccessfulGoogleTokenRetrievalMessage,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }

        return new ResponseDto<string?>
        {
            IsSuccess = true,
            Message = MessageConstants.SuccessfulGoogleTokenRetrievalMessage,
            HttpStatusCode = HttpStatusCode.OK
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
            user.GoogleToken.RefreshToken = googleTokenResponseDto.RefreshToken;
            user.GoogleToken.Scope = googleTokenResponseDto.Scope;
            user.GoogleToken.TokenType = googleTokenResponseDto.TokenType;
            user.GoogleToken.IdToken = googleTokenResponseDto.IdToken;
            user.GoogleToken.MarkAsUpdated();

            var updateCount = await _googleTokenRepository.SaveChangesAsync(cancellationToken);

            return updateCount > 0;
        }

        var googleToken = googleTokenResponseDto.ToGoogleTokenDomainEntity();

        googleToken.UserId = user.Id;

        await _googleTokenRepository.CreateAsync(googleToken, cancellationToken);

        var insertCount = await _googleTokenRepository.SaveChangesAsync(cancellationToken);

        return insertCount > 0;
    }

    private void StoreStateAndUserIdInMemoryCache(string state, long userId) =>
        _memoryCache.Set(state, userId, _memoryCacheEntryOptions);

    private long? RetrieveUserIdFromMemoryCache(string state) =>
        _memoryCache.TryGetValue<long>(state, out var userId) ? userId : null;

    private string GenerateEd25519Jwt(User user)
    {
        var jwtConfigurationDto = _configuration.GetSection(ConfigurationConstants.Ed25519JwtConfigurationSectionKey).Get<JwtConfigurationDto>()!;

        var utcNowUnixTime = DateTimeOffset
            .UtcNow
            .ToUnixTimeSeconds()
            .ToString(CultureInfo.InvariantCulture);

        var expirationDateUnixTime = DateTimeOffset
            .UtcNow
            .AddSeconds(jwtConfigurationDto.TokenLifetimeInSeconds)
            .ToUnixTimeSeconds()
            .ToString(CultureInfo.InvariantCulture);

        var claims = new Claim[]
        {
            new(JwtRegisteredClaimNames.Iss, jwtConfigurationDto.Issuer),
            new(JwtRegisteredClaimNames.Aud, jwtConfigurationDto.Audience),
            new(JwtRegisteredClaimNames.Iat, utcNowUnixTime, ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Nbf, utcNowUnixTime, ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Exp, expirationDateUnixTime, ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtExtendedConstants.JwtUsernameClaimType, user.Username),
            new(JwtExtendedConstants.JwtUuidClaimType, user.Uuid.ToString())
        };

        var jwtHeader = new EdDSAJwtHeader();

        var payload = new JwtPayload(claims);

        var headerJson = JsonSerializer.Serialize(jwtHeader, _jsonSerializerOptions);
        var payloadJson = JsonSerializer.Serialize(payload, _jsonSerializerOptions);

        var encodedHeader = Base64UrlEncoder.Encode(headerJson);
        var encodedPayload = Base64UrlEncoder.Encode(payloadJson);

        var signingInput = encodedHeader + '.' + encodedPayload;

        var signer = new Ed25519Signer();

        var messageBytes = Encoding.UTF8.GetBytes(signingInput);

        var privateKey = CryptographyHelper.LoadPrivateKeyFromString(jwtConfigurationDto.PrivateKey);

        signer.Init(true, privateKey);
        signer.BlockUpdate(messageBytes, 0, messageBytes.Length);

        var signatureBytes = signer.GenerateSignature();

        var signature = Base64UrlEncoder.Encode(signatureBytes);

        var jwt = signingInput + '.' + signature;

        return jwt;
    }

    private async Task<User?> ValidateAndGetUserByCredentialsAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        if (!RegexValidator.PasswordRegex().IsMatch(loginDto.Password))
        {
            return null;
        }

        User? user;

        if (RegexValidator.UsernameRegex().IsMatch(loginDto.UsernameOrEmail))
        {
            user = await GetByUsernameAsync(loginDto.UsernameOrEmail, cancellationToken);
        }
        else
        {
            user = await GetByEmailAsync(loginDto.UsernameOrEmail, cancellationToken);
        }

        if (user is null)
        {
            return null;
        }

        var isPasswordValid = CryptographyHelper.ValidatePassword(loginDto.Password, user.PasswordHash);

        return !isPasswordValid ? null : user;
    }

    private bool IsSignedIn() =>
        _httpContextAccessor.HttpContext!.User.Identity is not null && _httpContextAccessor.HttpContext!.User.Identity.IsAuthenticated;

    private async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var result = await _userRepository.GetAllAsync(
            1,
            1,
            user => user.Username == username,
            null,
            cancellationToken);

        return result.SingleOrDefault();
    }

    private async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var result = await _userRepository.GetAllAsync(
            1,
            1,
            user => user.Email == email,
            null,
            cancellationToken);

        return result.SingleOrDefault();
    }
}