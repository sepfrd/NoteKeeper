namespace NoteKeeper.DataAccess.Entities;

public abstract class DomainEntity
{
    protected DomainEntity()
    {
        CreatedAt = UpdatedAt = DateTime.UtcNow;
        Uuid = Guid.NewGuid();
    }

    public long Id { get; set; }

    public Guid Uuid { get; init; }

    public DateTime CreatedAt { get; private init; }

    public DateTime UpdatedAt { get; set; }

    public void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
}