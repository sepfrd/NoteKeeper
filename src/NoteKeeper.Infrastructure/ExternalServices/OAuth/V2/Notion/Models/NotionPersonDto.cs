using System.Text.Json.Serialization;
using NoteKeeper.Infrastructure.Common.Constants;

namespace NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Notion.Models;

public record NotionPersonDto
{
    [JsonPropertyName(CustomOAuthConstants.NotionEmailJsonPropertyName)]
    public string? Email { get; set; }
}