using System.Text.Json.Serialization;
using NoteKeeper.Business.Constants;

namespace NoteKeeper.Business.Dtos.Notion;

public record NotionPersonDto
{
    [JsonPropertyName(CustomOAuthConstants.NotionEmailJsonPropertyName)]
    public string? Email { get; set; }
}