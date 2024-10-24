using System.Text.Json.Serialization;
using NoteKeeper.Business.Constants;

namespace NoteKeeper.Business.Dtos.Notion;

public record NotionUserDto
{
    [JsonPropertyName(OAuth2Constants.NotionObjectJsonPropertyName)]
    public string? Object { get; set; }

    [JsonPropertyName(OAuth2Constants.NotionIdJsonPropertyName)]
    public string? Id { get; set; }

    [JsonPropertyName(OAuth2Constants.NotionNameJsonPropertyName)]
    public string? Name { get; set; }

    [JsonPropertyName(OAuth2Constants.NotionAvatarUrlJsonPropertyName)]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName(OAuth2Constants.NotionTypeJsonPropertyName)]
    public string? Type { get; set; }

    [JsonPropertyName(OAuth2Constants.NotionPersonJsonPropertyName)]
    public NotionPersonDto? Person { get; set; }
}