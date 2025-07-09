using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NoteKeeper.Application.Features.Notes.Dtos;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Application.Interfaces.CQRS;
using NoteKeeper.Domain.Common;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Shared.Constants;
using NoteKeeper.Shared.Resources;

namespace NoteKeeper.Application.Features.Notes.Commands.CreateNote;

public class CreateNoteCommandHandler : ICommandHandler<CreateNoteCommand, DomainResult<NoteDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMappingService _mappingService;
    private readonly ILogger<CreateNoteCommandHandler> _logger;

    public CreateNoteCommandHandler(
        IUnitOfWork unitOfWork,
        IMappingService mappingService,
        ILogger<CreateNoteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mappingService = mappingService;
        _logger = logger;
    }

    public async Task<DomainResult<NoteDto>> HandleAsync(CreateNoteCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdentityAsync(command.UserUuid, cancellationToken);

        if (user is null)
        {
            return DomainResult<NoteDto>.CreateFailure(
                ErrorMessages.Unauthorized,
                StatusCodes.Status401Unauthorized);
        }

        var note = _mappingService.Map<CreateNoteCommand, Note>(command);

        if (note is null)
        {
            _logger.LogCritical(LogMessages.MappingErrorTemplate, typeof(CreateNoteCommand), typeof(Note));

            return DomainResult<NoteDto>.CreateFailure(
                ErrorMessages.InternalServerError,
                StatusCodes.Status500InternalServerError);
        }

        note.UserId = user.Id;

        await _unitOfWork.NoteRepository.CreateAsync(note, cancellationToken);

        var commitResult = await _unitOfWork.CommitChangesAsync(cancellationToken);

        if (commitResult < 1)
        {
            _logger.LogCritical(LogMessages.DatabasePersistenceErrorTemplate, typeof(Note), StringConstants.CreateActionName);

            return DomainResult<NoteDto>.CreateFailure(
                ErrorMessages.InternalServerError,
                StatusCodes.Status500InternalServerError);
        }

        note.User = user;

        var noteResponseDto = _mappingService.Map<Note, NoteDto>(note);

        if (noteResponseDto is null)
        {
            _logger.LogCritical(LogMessages.MappingErrorTemplate, typeof(Note), typeof(NoteDto));

            return DomainResult<NoteDto>.CreateFailure(
                ErrorMessages.InternalServerError,
                StatusCodes.Status500InternalServerError);
        }

        var message = string.Format(SuccessMessages.CreateTemplate, nameof(Note).Humanize(LetterCasing.LowerCase));

        return DomainResult<NoteDto>.CreateSuccess(message, StatusCodes.Status201Created, noteResponseDto);
    }
}