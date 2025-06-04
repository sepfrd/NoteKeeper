namespace NoteKeeper.DataAccess.Entities;

public abstract class DomainEntity
{
    protected DomainEntity()
    {
        CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
        Uuid = Guid.CreateVersion7();
    }

    public long Id { get; set; }

    public Guid Uuid { get; init; }

    public DateTimeOffset CreatedAt { get; private init; }

    public DateTimeOffset UpdatedAt { get; set; }

    public void MarkAsUpdated() => UpdatedAt = DateTimeOffset.UtcNow;
}