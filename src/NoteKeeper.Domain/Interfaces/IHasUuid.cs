namespace NoteKeeper.Domain.Interfaces;

public interface IHasUuid
{
    Guid Uuid { get; init; }
}