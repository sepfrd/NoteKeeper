using NoteKeeper.Shared.Cqrs.Commands;
using NoteKeeper.Shared.Results;

namespace NoteKeeper.Application.Features.Notes.Commands.DeleteByUuid;

public record DeleteNoteCommand(Guid NoteUuid, Guid UserUuid) : ICommand<DomainResult>;