using NoteKeeper.Application.Features.Notes.Dtos;
using NoteKeeper.Shared.Cqrs.Queries;
using NoteKeeper.Shared.Results;

namespace NoteKeeper.Application.Features.Notes.Queries.GetNoteByUuid;

public record GetSingleNoteQuery(Guid NoteUuid) : IQuery<DomainResult<NoteDto>>;