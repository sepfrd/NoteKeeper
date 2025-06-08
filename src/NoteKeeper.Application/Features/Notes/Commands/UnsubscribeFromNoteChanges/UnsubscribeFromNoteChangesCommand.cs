using NoteKeeper.Application.Interfaces.CQRS;

namespace NoteKeeper.Application.Features.Notes.Commands.UnsubscribeFromNoteChanges;

public record UnsubscribeFromNoteChangesCommand(Guid NoteUuid) : ICommand;