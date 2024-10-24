using System.Text.Json.Serialization;
using NoteKeeper.Business.Constants;

namespace NoteKeeper.Business.Dtos.Notion;

public record NotionOwnerDto
{
    [JsonPropertyName(OAuth2Constants.NotionTypeJsonPropertyName)]
    public string? Type { get; set; }

    [JsonPropertyName(OAuth2Constants.NotionUserJsonPropertyName)]
    public NotionUserDto? User { get; set; }
}