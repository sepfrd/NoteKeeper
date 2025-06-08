using System.Text.Json.Serialization;
using NoteKeeper.Infrastructure.Common.Constants;

namespace NoteKeeper.Infrastructure.ExternalServices.Notion.Data;

public record NotionTokenRequestBodyDto
{
    [JsonPropertyName(CustomOAuthConstants.GrantTypeJsonPropertyName)]
    public required string GrantType { get; set; }

    [JsonPropertyName(CustomOAuthConstants.CodeJsonPropertyName)]
    public required string Code { get; set; }

    [JsonPropertyName(CustomOAuthConstants.RedirectUriJsonPropertyName)]
    public required string RedirectUri { get; set; }
}