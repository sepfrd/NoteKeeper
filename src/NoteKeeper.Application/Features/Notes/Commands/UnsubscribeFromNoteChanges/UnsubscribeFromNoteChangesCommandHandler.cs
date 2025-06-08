using Microsoft.AspNetCore.Http;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Application.Interfaces.CQRS;
using NoteKeeper.Domain.Common;

namespace NoteKeeper.Application.Features.Notes.Commands.UnsubscribeFromNoteChanges;

public class UnsubscribeFromNoteChangesCommandHandler : ICommandHandler<UnsubscribeFromNoteChangesCommand, DomainResult>
{
    private readonly INoteService _noteService;

    public UnsubscribeFromNoteChangesCommandHandler(INoteService noteService)
    {
        _noteService = noteService;
    }

    public async Task<DomainResult> HandleAsync(UnsubscribeFromNoteChangesCommand command, CancellationToken cancellationToken)
    {
        await _noteService.UnsubscribeFromNoteChangesAsync(command.NoteUuid);

        return DomainResult.CreateBaseSuccess(null, StatusCodes.Status200OK);
    }
}