using NoteKeeper.Application.Interfaces.CQRS;

namespace NoteKeeper.Application.Features.Notes.Commands.UpdateByUuid;

public record UpdateNoteCommand(Guid NoteUuid, Guid UserUuid, string NewTitle, string NewContent) : ICommand;