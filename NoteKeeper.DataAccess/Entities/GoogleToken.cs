namespace NoteKeeper.DataAccess.Entities;

public class GoogleToken : DomainEntity
{
    public required string AccessToken { get; set; }

    public required DateTime ExpiresAt { get; set; }

    public required string Scope { get; set; }

    public required string TokenType { get; set; }

    public string? IdToken { get; set; }

    public string? RefreshToken { get; set; }

    public long UserId { get; set; }

    public User? User { get; set; }
}