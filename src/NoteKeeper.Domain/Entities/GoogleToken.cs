namespace NoteKeeper.Domain.Entities;

public class GoogleToken : DomainEntity
{
    public required string AccessToken { get; set; }

    public required DateTimeOffset ExpiresAt { get; set; }

    public required string Scope { get; set; }

    public required string TokenType { get; set; }

    public required string RefreshToken { get; set; }

    public string? IdToken { get; set; }

    public long UserId { get; set; }

    public User? User { get; set; }

    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;
}