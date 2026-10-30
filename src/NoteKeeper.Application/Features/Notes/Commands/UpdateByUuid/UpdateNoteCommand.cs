using NoteKeeper.Application.Features.Notes.Dtos;
using NoteKeeper.Shared.Cqrs.Commands;
using NoteKeeper.Shared.Results;

namespace NoteKeeper.Application.Features.Notes.Commands.UpdateByUuid;

public record UpdateNoteCommand(Guid NoteUuid, Guid UserUuid, string NewTitle, string NewContent) : ICommand<DomainResult<NoteDto>>;