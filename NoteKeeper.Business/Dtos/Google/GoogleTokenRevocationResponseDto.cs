using System.Text.Json.Serialization;
using NoteKeeper.Business.Constants;

namespace NoteKeeper.Business.Dtos.Google;

public record GoogleTokenRevocationResponseDto
{
    [JsonPropertyName(CustomOAuthConstants.ErrorDescriptionJsonPropertyName)]
    public string? ErrorDescription { get; init; }
}