using NoteKeeper.Application.Interfaces;
using NoteKeeper.Application.Interfaces.CQRS;
using NoteKeeper.Domain.Entities;

namespace NoteKeeper.Application.Features.Notes.Queries.GetAllNotes;

public record GetAllNotesByFilterQuery(IFilterBase<Note>? Filter, uint PageNumber, uint PageSize) : IQuery;