namespace NoteKeeper.Infrastructure.ExternalServices.Notion.Data;

public class NotionToken
{
    public long Id { get; set; }

    public required string AccessToken { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public required string TokenType { get; set; }

    public string? BotId { get; set; }

    public string? WorkspaceName { get; set; }

    public string? WorkspaceIconUrl { get; set; }

    public string? WorkspaceId { get; set; }

    public string? NotionId { get; set; }

    public string? Name { get; set; }

    public string? AvatarUrl { get; set; }

    public string? NotionEmail { get; set; }

    public long UserId { get; set; }

    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;
}