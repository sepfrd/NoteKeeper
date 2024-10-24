using System.Text.Json.Serialization;
using NoteKeeper.Business.Constants;
using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.Business.Dtos.Google;

public record GoogleTokenResponseDto
{
    [JsonPropertyName(OAuth2Constants.AccessTokenJsonPropertyName)]
    public required string AccessToken { get; init; }

    [JsonPropertyName(OAuth2Constants.ExpiresInJsonPropertyName)]
    public int ExpiresIn { get; init; }

    [JsonPropertyName(OAuth2Constants.ScopeJsonPropertyName)]
    public required string Scope { get; init; }

    [JsonPropertyName(OAuth2Constants.TokenTypeJsonPropertyName)]
    public required string TokenType { get; init; }

    [JsonPropertyName(OAuth2Constants.RefreshTokenJsonPropertyName)]
    public string? RefreshToken { get; init; }

    [JsonPropertyName(OAuth2Constants.IdTokenJsonPropertyName)]
    public string? IdToken { get; init; }

    public GoogleToken ToGoogleTokenDomainEntity() => new()
    {
        AccessToken = AccessToken,
        ExpiresAt = DateTime.UtcNow.AddSeconds(ExpiresIn),
        RefreshToken = RefreshToken!,
        Scope = Scope,
        TokenType = TokenType,
        IdToken = IdToken
    };
}