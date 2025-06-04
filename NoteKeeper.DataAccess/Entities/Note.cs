using NoteKeeper.DataAccess.Enums;

namespace NoteKeeper.DataAccess.Entities;

public class Note : DomainEntity
{
    public required string Title { get; set; }

    public required string Content { get; set; }

    public long UserId { get; set; }

    public User? User { get; set; }

    public NoteOrigin Origin { get; set; } = NoteOrigin.Native;
}