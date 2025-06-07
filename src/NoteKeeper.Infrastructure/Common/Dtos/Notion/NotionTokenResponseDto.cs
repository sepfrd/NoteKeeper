using System.Text.Json.Serialization;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Infrastructure.Common.Constants;

namespace NoteKeeper.Infrastructure.Common.Dtos.Notion;

public record NotionTokenResponseDto
{
    [JsonPropertyName(CustomOAuthConstants.AccessTokenJsonPropertyName)]
    public required string AccessToken { get; set; }

    [JsonPropertyName(CustomOAuthConstants.TokenTypeJsonPropertyName)]
    public required string TokenType { get; set; }

    [JsonPropertyName(CustomOAuthConstants.NotionBotIdJsonPropertyName)]
    public string? BotId { get; set; }

    [JsonPropertyName(CustomOAuthConstants.NotionWorkspaceNameJsonPropertyName)]
    public string? WorkspaceName { get; set; }

    [JsonPropertyName(CustomOAuthConstants.NotionWorkspaceIconJsonPropertyName)]
    public string? WorkspaceIcon { get; set; }

    [JsonPropertyName(CustomOAuthConstants.NotionWorkspaceIdJsonPropertyName)]
    public string? WorkspaceId { get; set; }

    [JsonPropertyName(CustomOAuthConstants.NotionOwnerJsonPropertyName)]
    public NotionOwnerDto? Owner { get; set; }

    [JsonPropertyName(CustomOAuthConstants.NotionDuplicatedTemplateIdJsonPropertyName)]
    public string? DuplicatedTemplateId { get; set; }

    [JsonPropertyName(CustomOAuthConstants.NotionRequestIdJsonPropertyName)]
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