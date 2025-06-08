using NoteKeeper.Domain.Entities;

namespace NoteKeeper.Infrastructure.ExternalServices.Google.Data;

public class GoogleToken
{
    public long Id { get; set; }

    public required string AccessToken { get; set; }

    public required DateTimeOffset ExpiresAt { get; set; }

    public required string Scope { get; set; }

    public required string TokenType { get; set; }

    public required string RefreshToken { get; set; }

    public string? IdToken { get; set; }

    public long UserId { get; set; }

    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;
}