using NoteKeeper.Domain.Interfaces;

namespace NoteKeeper.Infrastructure.ExternalServices.Google.Data;

public class GoogleToken : IEntity<long>, IAuditable
{
    public GoogleToken()
    {
        CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
    }

    public long Id { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; set; }

    public void MarkAsUpdated() => UpdatedAt = DateTimeOffset.UtcNow;

    public required string AccessToken { get; set; }

    public required DateTimeOffset ExpiresAt { get; set; }

    public required string Scope { get; set; }

    public required string TokenType { get; set; }

    public required string RefreshToken { get; set; }

    public string? IdToken { get; set; }

    public long UserId { get; set; }

    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;
}