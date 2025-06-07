using NoteKeeper.Application.Interfaces.CQRS;

namespace NoteKeeper.Application.Features.Notes.Commands.DeleteByUuid;

public record DeleteNoteCommand(Guid NoteUuid, Guid UserUuid) : ICommand;