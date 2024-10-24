using System.Text.Json.Serialization;
using NoteKeeper.Business.Constants;

namespace NoteKeeper.Business.Dtos.Google;

public record GoogleTokenRevocationResponseDto
{
    [JsonPropertyName(OAuth2Constants.ErrorDescriptionJsonPropertyName)]
    public string? ErrorDescription { get; init; }
}