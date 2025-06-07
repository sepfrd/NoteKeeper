using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Infrastructure.Common.Constants;
using NoteKeeper.Infrastructure.Common.Dtos;
using NoteKeeper.Infrastructure.Common.Dtos.Configurations;
using NoteKeeper.Shared.Utilities;
using Org.BouncyCastle.Crypto.Signers;

namespace NoteKeeper.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IRedisService _redisService;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly AppOptions _appOptions;

    public TokenService(
        IRedisService redisService,
        JsonSerializerOptions jsonSerializerOptions,
        IOptions<AppOptions> appOptions)
    {
        _redisService = redisService;
        _jsonSerializerOptions = jsonSerializerOptions;
        _appOptions = appOptions.Value;
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

        var publicKey = CryptographyHelper.LoadPublicKeyFromString(_appOptions.JwtOptions.PublicKey);

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

    public string GenerateEd25519Jwt(User user)
    {
        var utcNowUnixTime = DateTimeOffset
            .UtcNow
            .ToUnixTimeSeconds()
            .ToString(CultureInfo.InvariantCulture);

        var expirationDateUnixTime = DateTimeOffset
            .UtcNow
            .AddSeconds(_appOptions.JwtOptions.TokenLifetimeInSeconds)
            .ToUnixTimeSeconds()
            .ToString(CultureInfo.InvariantCulture);

        var claims = new Claim[]
        {
            new(JwtRegisteredClaimNames.Iss, _appOptions.JwtOptions.Issuer),
            new(JwtRegisteredClaimNames.Aud, _appOptions.JwtOptions.Audience),
            new(JwtRegisteredClaimNames.Iat, utcNowUnixTime, ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Nbf, utcNowUnixTime, ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Exp, expirationDateUnixTime, ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtExtendedConstants.JwtUsernameClaimType, user.Username),
            new(JwtExtendedConstants.JwtUuidClaimType, user.Uuid.ToString())
        };

        var jwtHeader = new EdDsaJwtHeader();

        var payload = new JwtPayload(claims);

        var headerJson = JsonSerializer.Serialize(jwtHeader, _jsonSerializerOptions);
        var payloadJson = JsonSerializer.Serialize(payload, _jsonSerializerOptions);

        var encodedHeader = Base64UrlEncoder.Encode(headerJson);
        var encodedPayload = Base64UrlEncoder.Encode(payloadJson);

        var signingInput = encodedHeader + '.' + encodedPayload;

        var signer = new Ed25519Signer();

        var messageBytes = Encoding.UTF8.GetBytes(signingInput);

        var privateKey = CryptographyHelper.LoadPrivateKeyFromString(_appOptions.JwtOptions.PrivateKey);

        signer.Init(true, privateKey);
        signer.BlockUpdate(messageBytes, 0, messageBytes.Length);

        var signatureBytes = signer.GenerateSignature();

        var signature = Base64UrlEncoder.Encode(signatureBytes);

        var jwt = signingInput + '.' + signature;

        return jwt;
    }

    public async Task<string> GenerateNewRefreshTokenAsync(string userIdString)
    {
        var existingReverseRefreshTokenKey = string.Format(RedisConstants.ReverseRefreshTokenStringKeyTemplate, userIdString);

        var existingReverseRefreshToken = await _redisService.GetStringAsync(
            existingReverseRefreshTokenKey,
            _appOptions.RedisOptions.Databases.Auth);

        if (!existingReverseRefreshToken.IsNullOrEmpty)
        {
            var existingRefreshTokenKey = string.Format(RedisConstants.RefreshTokenStringKeyTemplate, existingReverseRefreshToken);

            await _redisService.DeleteKeyAsync(existingRefreshTokenKey, _appOptions.RedisOptions.Databases.Auth);
        }

        var refreshToken = Guid.CreateVersion7().ToString();

        var redisKey = string.Format(RedisConstants.RefreshTokenStringKeyTemplate, refreshToken);
        var reverseRedisKey = string.Format(RedisConstants.ReverseRefreshTokenStringKeyTemplate, userIdString);

        var refreshTokenLifeTimeInMinutes = _appOptions.JwtOptions.RefreshTokenLifeTimeInMinutes;
        var redisAuthDatabase = _appOptions.RedisOptions.Databases.Auth;

        await _redisService.SetStringAsync(
            redisKey,
            userIdString,
            TimeSpan.FromMinutes(refreshTokenLifeTimeInMinutes),
            redisAuthDatabase);

        await _redisService.SetStringAsync(
            reverseRedisKey,
            refreshToken,
            TimeSpan.FromMinutes(refreshTokenLifeTimeInMinutes),
            redisAuthDatabase);

        return refreshToken;
    }
}