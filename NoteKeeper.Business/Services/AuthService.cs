using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NoteKeeper.Business.Constants;
using NoteKeeper.Business.Dtos;
using NoteKeeper.Business.Interfaces;
using NoteKeeper.Business.Utilities;
using NoteKeeper.DataAccess.Entities;
using NoteKeeper.DataAccess.Enums;
using NoteKeeper.DataAccess.Interfaces;
using Org.BouncyCastle.Crypto.Signers;

namespace NoteKeeper.Business.Services;

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IUserRepository _userRepository;

    public AuthService(
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        JsonSerializerOptions jsonSerializerOptions)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
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

        var user = await GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail, cancellationToken);

        if (user is null || user.RegistrationType != RegistrationType.Direct)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.InvalidCredentialsMessage,
                HttpStatusCode = HttpStatusCode.BadRequest
            };
        }

        if (!RegexValidator.PasswordRegex().IsMatch(loginDto.Password))
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = MessageConstants.InvalidCredentialsMessage,
                HttpStatusCode = HttpStatusCode.BadRequest
            };
        }

        var isPasswordValid = CryptographyHelper.ValidatePassword(loginDto.Password, user.PasswordHash!);

        if (!isPasswordValid)
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
        var userUuid = _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtExtendedConstants.JwtUuidClaimType);

        if (userUuid is null)
        {
            return null;
        }

        var user = await _userRepository.GetByUuidAsync(Guid.Parse(userUuid), include, cancellationToken);

        return user;
    }

    public string GenerateEd25519Jwt(User user)
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

    private bool IsSignedIn() =>
        _httpContextAccessor.HttpContext!.User.Identity is not null && _httpContextAccessor.HttpContext!.User.Identity.IsAuthenticated;

    private async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
    {
        User? user;

        if (RegexValidator.UsernameRegex().IsMatch(usernameOrEmail))
        {
            user = await _userRepository.GetByUsernameAsync(usernameOrEmail, null, cancellationToken);
        }
        else
        {
            user = await _userRepository.GetByEmailAsync(usernameOrEmail, null, cancellationToken);
        }

        return user;
    }
}