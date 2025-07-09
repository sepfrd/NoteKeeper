using NoteKeeper.Application.Interfaces.CQRS;

namespace NoteKeeper.Application.Features.Notes.Queries.GetNoteByUuid;

public record GetSingleNoteQuery(Guid NoteUuid) : IQuery;