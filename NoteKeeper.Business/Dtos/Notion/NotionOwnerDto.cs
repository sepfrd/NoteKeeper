using System.Text.Json.Serialization;
using NoteKeeper.Business.Constants;

namespace NoteKeeper.Business.Dtos.Notion;

public record NotionOwnerDto
{
    [JsonPropertyName(CustomOAuthConstants.NotionTypeJsonPropertyName)]
    public string? Type { get; set; }

    [JsonPropertyName(CustomOAuthConstants.NotionUserJsonPropertyName)]
    public NotionUserDto? User { get; set; }
}