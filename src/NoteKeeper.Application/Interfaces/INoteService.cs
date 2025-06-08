namespace NoteKeeper.Application.Interfaces;

public interface INoteService
{
    Task SubscribeToNoteChangesAsync(Guid noteUuid);

    Task UnsubscribeFromNoteChangesAsync(Guid noteUuid);
}