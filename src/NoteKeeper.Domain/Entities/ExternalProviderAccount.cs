using NoteKeeper.Domain.Enums;

namespace NoteKeeper.Domain.Entities;

public class ExternalProviderAccount : DomainEntity
{
    public required string ProviderName { get; set; }

    public ProviderType ProviderType { get; set; }

    public DateTimeOffset LinkedAt { get; set; } = DateTimeOffset.UtcNow;

    public long UserId { get; set; }

    public User? User { get; set; }
}