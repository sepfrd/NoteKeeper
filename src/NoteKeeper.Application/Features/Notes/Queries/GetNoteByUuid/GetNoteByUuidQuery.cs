using NoteKeeper.Application.Interfaces.CQRS;

namespace NoteKeeper.Application.Features.Notes.Queries.GetNoteByUuid;

public record GetNoteByUuidQuery(Guid NoteUuid) : IQuery;