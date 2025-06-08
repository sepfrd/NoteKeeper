using Microsoft.AspNetCore.Http;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Application.Interfaces.CQRS;
using NoteKeeper.Domain.Common;

namespace NoteKeeper.Application.Features.Notes.Commands.SubscribeToNoteChanges;

public class SubscribeToNoteChangesCommandHandler : ICommandHandler<SubscribeToNoteChangesCommand, DomainResult>
{
    private readonly INoteService _noteService;

    public SubscribeToNoteChangesCommandHandler(INoteService noteService)
    {
        _noteService = noteService;
    }

    public async Task<DomainResult> HandleAsync(SubscribeToNoteChangesCommand command, CancellationToken cancellationToken)
    {
        await _noteService.SubscribeToNoteChangesAsync(command.NoteUuid);

        return DomainResult.CreateBaseSuccess(null, StatusCodes.Status200OK);
    }
}