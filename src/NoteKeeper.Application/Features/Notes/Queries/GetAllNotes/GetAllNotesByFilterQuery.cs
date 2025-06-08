using NoteKeeper.Application.Features.Notes.Dtos;
using NoteKeeper.Application.Interfaces.CQRS;

namespace NoteKeeper.Application.Features.Notes.Queries.GetAllNotes;

public record GetAllNotesByFilterQuery(NoteFilterDto? FilterDto, uint PageNumber, uint PageSize) : IQuery;