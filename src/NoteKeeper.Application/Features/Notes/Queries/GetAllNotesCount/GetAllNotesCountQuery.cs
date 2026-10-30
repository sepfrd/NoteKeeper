using NoteKeeper.Shared.Cqrs.Queries;
using NoteKeeper.Shared.Results;

namespace NoteKeeper.Application.Features.Notes.Queries.GetAllNotesCount;

public record GetAllNotesCountQuery : IQuery<DomainResult<long>>;