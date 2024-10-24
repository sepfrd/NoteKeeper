using System.Text.Json.Serialization;
using NoteKeeper.Business.Constants;

namespace NoteKeeper.Business.Dtos.Notion;

public record NotionTokenRequestBodyDto
{
    [JsonPropertyName(OAuth2Constants.GrantTypeJsonPropertyName)]
    public required string GrantType { get; set; }

    [JsonPropertyName(OAuth2Constants.CodeJsonPropertyName)]
    public required string Code { get; set; }

    [JsonPropertyName(OAuth2Constants.RedirectUriJsonPropertyName)]
    public required string RedirectUri { get; set; }
};