using System.Text.Json.Serialization;
using NoteKeeper.Infrastructure.Common.Constants;

namespace NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Notion.Data;

public record NotionPersonDto
{
    [JsonPropertyName(CustomOAuthConstants.NotionEmailJsonPropertyName)]
    public string? Email { get; set; }
}