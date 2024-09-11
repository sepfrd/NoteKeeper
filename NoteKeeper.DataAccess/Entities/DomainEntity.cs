namespace NoteKeeper.DataAccess.Entities;

public abstract class DomainEntity
{
    protected DomainEntity()
    {
        CreatedAt = UpdatedAt = DateTime.Now;
    }

    public long Id { get; set; }

    public DateTime CreatedAt { get; private init; }

    public DateTime UpdatedAt { get; set; }

    public void MarkAsUpdated() => UpdatedAt = DateTime.Now;
}