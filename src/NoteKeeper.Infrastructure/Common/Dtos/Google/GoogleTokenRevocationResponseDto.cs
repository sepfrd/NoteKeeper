using System.Text.Json.Serialization;
using NoteKeeper.Infrastructure.Common.Constants;

namespace NoteKeeper.Infrastructure.Common.Dtos.Google;

public record GoogleTokenRevocationResponseDto
{
    [JsonPropertyName(CustomOAuthConstants.ErrorDescriptionJsonPropertyName)]
    public string? ErrorDescription { get; init; }
}