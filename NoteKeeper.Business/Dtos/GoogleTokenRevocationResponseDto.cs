using System.Text.Json.Serialization;
using NoteKeeper.Business.Constants;

namespace NoteKeeper.Business.Dtos;

public record GoogleTokenRevocationResponseDto
{
    [JsonPropertyName(GoogleOAuth2Constants.ErrorDescriptionJsonPropertyName)]
    public string? ErrorDescription { get; init; }
}