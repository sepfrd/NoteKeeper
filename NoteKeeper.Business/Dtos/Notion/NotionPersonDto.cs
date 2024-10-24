using System.Text.Json.Serialization;
using NoteKeeper.Business.Constants;

namespace NoteKeeper.Business.Dtos.Notion;

public record NotionPersonDto
{
    [JsonPropertyName(OAuth2Constants.NotionEmailJsonPropertyName)]
    public string? Email { get; set; }
};