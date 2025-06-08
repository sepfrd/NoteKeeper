using NoteKeeper.Domain.Interfaces;

namespace NoteKeeper.Domain.Entities;

public abstract class DomainEntity : IEntity<long>, IAuditable, IHasUuid
{
    protected DomainEntity()
    {
        CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
        Uuid = Guid.CreateVersion7();
    }

    public long Id { get; set; }

    public Guid Uuid { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; set; }

    public void MarkAsUpdated() => UpdatedAt = DateTimeOffset.UtcNow;
}