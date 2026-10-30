using NoteKeeper.Application.Features.Notes.Dtos;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Shared.Cqrs.Queries;
using NoteKeeper.Shared.Results;

namespace NoteKeeper.Application.Features.Notes.Queries.GetAllNotes;

public record GetAllNotesByFilterQuery(
    IFilterBase<Note>? Filter,
    uint PageNumber,
    uint PageSize)
    : IQuery<PaginatedDomainResult<IEnumerable<NoteDto>>>;