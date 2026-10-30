using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Shared.Constants;
using NoteKeeper.Shared.Cqrs.Commands;
using NoteKeeper.Shared.Resources;
using NoteKeeper.Shared.Results;

namespace NoteKeeper.Application.Features.Notes.Commands.DeleteByUuid;

public class DeleteNoteCommandHandler : ICommandHandler<DeleteNoteCommand, DomainResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteNoteCommandHandler> _logger;

    public DeleteNoteCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteNoteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DomainResult> HandleAsync(DeleteNoteCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetByIdentityAsync(command.UserUuid, cancellationToken);

        if (user is null)
        {
            return DomainResult.CreateBaseFailure(Errors.Unauthorized, StatusCodes.Status401Unauthorized);
        }

        var note = await _unitOfWork.NoteRepository.GetByIdentityAsync(command.NoteUuid, cancellationToken);

        if (note is null)
        {
            var notFoundMessage = string.Format(
                ErrorMessages.EntityNotFoundByUuidTemplate,
                nameof(Note).Humanize(LetterCasing.LowerCase),
                command.NoteUuid);

            return DomainResult.CreateBaseFailure(Errors.NotFound, StatusCodes.Status404NotFound, notFoundMessage);
        }

        if (note.UserId != user.Id)
        {
            var forbiddenMessage = string.Format(
                ErrorMessages.ForbiddenActionTemplate,
                StringConstants.DeleteActionName.Humanize(LetterCasing.LowerCase),
                nameof(Note).Humanize(LetterCasing.LowerCase));

            return DomainResult.CreateBaseFailure(Errors.Forbidden, StatusCodes.Status403Forbidden, forbiddenMessage);
        }

        _unitOfWork.NoteRepository.Delete(note);

        var commitResult = await _unitOfWork.CommitChangesAsync(cancellationToken);

        if (commitResult < 1)
        {
            _logger.LogCritical(LogMessages.DatabasePersistenceErrorTemplate, typeof(Note), StringConstants.DeleteActionName);

            return DomainResult.CreateBaseFailure(
                Errors.InternalServerError,
                StatusCodes.Status500InternalServerError);
        }

        var message = string.Format(SuccessMessages.DeleteTemplate, nameof(Note).Humanize(LetterCasing.LowerCase));

        return DomainResult.CreateBaseSuccess(message, StatusCodes.Status200OK);
    }
}