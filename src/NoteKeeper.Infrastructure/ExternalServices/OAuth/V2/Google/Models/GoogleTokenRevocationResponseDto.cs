using System.Text.Json.Serialization;
using NoteKeeper.Infrastructure.Common.Constants;

namespace NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google.Models;

public record GoogleTokenRevocationResponseDto
{
    [JsonPropertyName(CustomOAuthConstants.ErrorDescriptionJsonPropertyName)]
    public string? ErrorDescription { get; init; }
}