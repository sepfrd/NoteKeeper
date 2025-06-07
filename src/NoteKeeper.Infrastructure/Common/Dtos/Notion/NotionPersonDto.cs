using System.Text.Json.Serialization;
using NoteKeeper.Infrastructure.Common.Constants;

namespace NoteKeeper.Infrastructure.Common.Dtos.Notion;

public record NotionPersonDto
{
    [JsonPropertyName(CustomOAuthConstants.NotionEmailJsonPropertyName)]
    public string? Email { get; set; }
}