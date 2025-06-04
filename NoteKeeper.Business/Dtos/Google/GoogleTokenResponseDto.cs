using System.Text.Json.Serialization;
using NoteKeeper.Business.Constants;
using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.Business.Dtos.Google;

public record GoogleTokenResponseDto
{
    [JsonPropertyName(CustomOAuthConstants.AccessTokenJsonPropertyName)]
    public required string AccessToken { get; init; }

    [JsonPropertyName(CustomOAuthConstants.ExpiresInJsonPropertyName)]
    public int ExpiresIn { get; init; }

    [JsonPropertyName(CustomOAuthConstants.ScopeJsonPropertyName)]
    public required string Scope { get; init; }

    [JsonPropertyName(CustomOAuthConstants.TokenTypeJsonPropertyName)]
    public required string TokenType { get; init; }

    [JsonPropertyName(CustomOAuthConstants.RefreshTokenJsonPropertyName)]
    public string? RefreshToken { get; init; }

    [JsonPropertyName(CustomOAuthConstants.IdTokenJsonPropertyName)]
    public string? IdToken { get; init; }

    public GoogleToken ToGoogleTokenDomainEntity() => new()
    {
        AccessToken = AccessToken,
        ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(ExpiresIn),
        RefreshToken = RefreshToken!,
        Scope = Scope,
        TokenType = TokenType,
        IdToken = IdToken
    };
}