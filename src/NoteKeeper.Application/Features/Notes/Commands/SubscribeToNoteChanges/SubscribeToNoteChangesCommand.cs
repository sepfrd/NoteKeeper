using NoteKeeper.Application.Interfaces.CQRS;

namespace NoteKeeper.Application.Features.Notes.Commands.SubscribeToNoteChanges;

public record SubscribeToNoteChangesCommand : ICommand;