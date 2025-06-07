using System.Text.Json.Serialization;
using NoteKeeper.Infrastructure.Common.Constants;

namespace NoteKeeper.Infrastructure.Common.Dtos.Notion;

public record NotionUserDto
{
    [JsonPropertyName(CustomOAuthConstants.NotionObjectJsonPropertyName)]
    public string? Object { get; set; }

    [JsonPropertyName(CustomOAuthConstants.NotionIdJsonPropertyName)]
    public string? Id { get; set; }

    [JsonPropertyName(CustomOAuthConstants.NotionNameJsonPropertyName)]
    public string? Name { get; set; }

    [JsonPropertyName(CustomOAuthConstants.NotionAvatarUrlJsonPropertyName)]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName(CustomOAuthConstants.NotionTypeJsonPropertyName)]
    public string? Type { get; set; }

    [JsonPropertyName(CustomOAuthConstants.NotionPersonJsonPropertyName)]
    public NotionPersonDto? Person { get; set; }
}