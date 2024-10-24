using System.Text.Json.Serialization;
using NoteKeeper.Business.Constants;
using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.Business.Dtos.Notion;

public record NotionTokenResponseDto
{
    [JsonPropertyName(OAuth2Constants.AccessTokenJsonPropertyName)]
    public required string AccessToken { get; set; }

    [JsonPropertyName(OAuth2Constants.TokenTypeJsonPropertyName)]
    public required string TokenType { get; set; }

    [JsonPropertyName(OAuth2Constants.NotionBotIdJsonPropertyName)]
    public string? BotId { get; set; }

    [JsonPropertyName(OAuth2Constants.NotionWorkspaceNameJsonPropertyName)]
    public string? WorkspaceName { get; set; }

    [JsonPropertyName(OAuth2Constants.NotionWorkspaceIconJsonPropertyName)]
    public string? WorkspaceIcon { get; set; }

    [JsonPropertyName(OAuth2Constants.NotionWorkspaceIdJsonPropertyName)]
    public string? WorkspaceId { get; set; }

    [JsonPropertyName(OAuth2Constants.NotionOwnerJsonPropertyName)]
    public NotionOwnerDto? Owner { get; set; }

    [JsonPropertyName(OAuth2Constants.NotionDuplicatedTemplateIdJsonPropertyName)]
    public string? DuplicatedTemplateId { get; set; }

    [JsonPropertyName(OAuth2Constants.NotionRequestIdJsonPropertyName)]
    public string? RequestId { get; set; }

    public NotionToken ToNotionTokenDomainEntity() => new()
    {
        AccessToken = AccessToken,
        TokenType = TokenType,
        BotId = BotId,
        WorkspaceName = WorkspaceName,
        WorkspaceIconUrl = WorkspaceIcon,
        WorkspaceId = WorkspaceId,
        NotionId = Owner?.User?.Id,
        Name = Owner?.User?.Name,
        AvatarUrl = Owner?.User?.AvatarUrl,
        NotionEmail = Owner?.User?.Person?.Email
    };
}