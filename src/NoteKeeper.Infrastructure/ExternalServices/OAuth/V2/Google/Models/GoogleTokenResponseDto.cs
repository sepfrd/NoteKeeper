using System.Text.Json.Serialization;
using NoteKeeper.Infrastructure.Common.Constants;

namespace NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google.Models;

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
}