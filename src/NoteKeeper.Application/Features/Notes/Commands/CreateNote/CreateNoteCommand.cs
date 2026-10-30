using NoteKeeper.Application.Features.Notes.Dtos;
using NoteKeeper.Shared.Cqrs.Commands;
using NoteKeeper.Shared.Results;

namespace NoteKeeper.Application.Features.Notes.Commands.CreateNote;

public record CreateNoteCommand(string Title, string Content, Guid UserUuid) : ICommand<DomainResult<NoteDto>>;