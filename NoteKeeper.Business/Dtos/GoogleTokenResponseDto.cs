using System.Text.Json.Serialization;
using NoteKeeper.Business.Constants;
using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.Business.Dtos;

public record GoogleTokenResponseDto
{
    [JsonPropertyName(GoogleOAuth2Constants.AccessTokenJsonPropertyName)]
    public required string AccessToken { get; init; }

    [JsonPropertyName(GoogleOAuth2Constants.ExpiresInJsonPropertyName)]
    public int ExpiresIn { get; init; }

    [JsonPropertyName(GoogleOAuth2Constants.ScopeJsonPropertyName)]
    public required string Scope { get; init; }

    [JsonPropertyName(GoogleOAuth2Constants.TokenTypeJsonPropertyName)]
    public required string TokenType { get; init; }

    [JsonPropertyName(GoogleOAuth2Constants.RefreshTokenJsonPropertyName)]
    public string? RefreshToken { get; init; }

    [JsonPropertyName(GoogleOAuth2Constants.IdTokenJsonPropertyName)]
    public string? IdToken { get; init; }

    public GoogleToken ToGoogleTokenDomainEntity() => new()
    {
        AccessToken = AccessToken,
        ExpiresAt = DateTime.UtcNow.AddSeconds(ExpiresIn),
        RefreshToken = RefreshToken,
        Scope = Scope,
        TokenType = TokenType,
        IdToken = IdToken,
    };
}