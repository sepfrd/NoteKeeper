using System.Text.Json.Serialization;
using NoteKeeper.Infrastructure.Common.Constants;

namespace NoteKeeper.Infrastructure.ExternalServices.Notion.Data;

public record NotionOwnerDto
{
    [JsonPropertyName(CustomOAuthConstants.NotionTypeJsonPropertyName)]
    public string? Type { get; set; }

    [JsonPropertyName(CustomOAuthConstants.NotionUserJsonPropertyName)]
    public NotionUserDto? User { get; set; }
}