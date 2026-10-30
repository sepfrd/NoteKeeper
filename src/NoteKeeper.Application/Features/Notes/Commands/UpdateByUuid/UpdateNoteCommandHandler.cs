using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NoteKeeper.Application.Features.Notes.Dtos;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Shared.Constants;
using NoteKeeper.Shared.Cqrs.Commands;
using NoteKeeper.Shared.Resources;
using NoteKeeper.Shared.Results;

namespace NoteKeeper.Application.Features.Notes.Commands.UpdateByUuid;

public class UpdateNoteCommandHandler : ICommandHandler<UpdateNoteCommand, DomainResult<NoteDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMappingService _mappingService;
    private readonly ILogger<UpdateNoteCommandHandler> _logger;

    public UpdateNoteCommandHandler(
        IUnitOfWork unitOfWork,
        IMappingService mappingService,
        ILogger<UpdateNoteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mappingService = mappingService;
        _logger = logger;
    }

    public async Task<DomainResult<NoteDto>> HandleAsync(UpdateNoteCommand request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetByIdentityAsync(request.UserUuid, cancellationToken);

        if (user is null)
        {
            return DomainResult<NoteDto>.CreateFailure(
                Errors.Unauthorized,
                StatusCodes.Status401Unauthorized);
        }

        var note = await _unitOfWork.NoteRepository.GetByIdentityAsync(request.NoteUuid, cancellationToken: cancellationToken);

        if (note is null)
        {
            var notFoundMessage = string.Format(
                ErrorMessages.EntityNotFoundByUuidTemplate,
                nameof(Note).Humanize(LetterCasing.LowerCase),
                request.NoteUuid);

            return DomainResult<NoteDto>.CreateFailure(Errors.NotFound, StatusCodes.Status404NotFound, notFoundMessage);
        }

        if (note.UserId != user.Id)
        {
            var forbiddenMessage = string.Format(
                ErrorMessages.ForbiddenActionTemplate,
                StringConstants.UpdateActionName.Humanize(LetterCasing.LowerCase),
                nameof(Note).Humanize(LetterCasing.LowerCase));

            return DomainResult<NoteDto>.CreateFailure(
                Errors.Forbidden,
                StatusCodes.Status403Forbidden,
                forbiddenMessage);
        }

        if (request.NewTitle == note.Title && request.NewContent == note.Content)
        {
            return DomainResult<NoteDto>.CreateFailure(
                Errors.BadRequest,
                StatusCodes.Status400BadRequest,
                ErrorMessages.IdenticalNewPropertyValues);
        }

        var updatedNote = _mappingService.Map(request, note);

        if (updatedNote is null)
        {
            _logger.LogCritical(
                LogMessages.MappingErrorTemplate,
                typeof(UpdateNoteCommand),
                typeof(Note));

            return DomainResult<NoteDto>.CreateFailure(
                Errors.InternalServerError,
                StatusCodes.Status500InternalServerError);
        }

        _unitOfWork.NoteRepository.Update(updatedNote);

        var commitResult = await _unitOfWork.CommitChangesAsync(cancellationToken);

        if (commitResult < 1)
        {
            _logger.LogCritical(LogMessages.DatabasePersistenceErrorTemplate, typeof(Note), StringConstants.UpdateActionName);

            return DomainResult<NoteDto>.CreateFailure(
                Errors.InternalServerError,
                StatusCodes.Status500InternalServerError);
        }

        var noteResponseDto = _mappingService.Map<Note, NoteDto>(updatedNote);

        if (noteResponseDto is null)
        {
            _logger.LogCritical(LogMessages.MappingErrorTemplate, typeof(Note), typeof(NoteDto));

            return DomainResult<NoteDto>.CreateFailure(
                Errors.InternalServerError,
                StatusCodes.Status500InternalServerError);
        }

        var message = string.Format(SuccessMessages.UpdateTemplate, nameof(Note).Humanize(LetterCasing.LowerCase));

        return DomainResult<NoteDto>.CreateSuccess(noteResponseDto, StatusCodes.Status200OK, message);
    }
}