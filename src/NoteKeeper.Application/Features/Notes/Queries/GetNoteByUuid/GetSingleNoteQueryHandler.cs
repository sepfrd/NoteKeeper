using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NoteKeeper.Application.Features.Notes.Dtos;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Shared.Cqrs.Queries;
using NoteKeeper.Shared.Resources;
using NoteKeeper.Shared.Results;

namespace NoteKeeper.Application.Features.Notes.Queries.GetNoteByUuid;

public class GetSingleNoteQueryHandler : IQueryHandler<GetSingleNoteQuery, DomainResult<NoteDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMappingService _mappingService;
    private readonly ILogger<GetSingleNoteQueryHandler> _logger;

    public GetSingleNoteQueryHandler(
        IUnitOfWork unitOfWork,
        IMappingService mappingService,
        ILogger<GetSingleNoteQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mappingService = mappingService;
        _logger = logger;
    }

    public async Task<DomainResult<NoteDto>> HandleAsync(GetSingleNoteQuery query, CancellationToken cancellationToken = default)
    {
        var note = await _unitOfWork.NoteRepository.GetByIdentityAsync(query.NoteUuid, cancellationToken);

        if (note is null)
        {
            var notFoundMessage = string.Format(
                ErrorMessages.EntityNotFoundByUuidTemplate,
                nameof(Note).Humanize(LetterCasing.LowerCase),
                query.NoteUuid);

            return DomainResult<NoteDto>.CreateFailure(Errors.NotFound, StatusCodes.Status404NotFound, notFoundMessage);
        }

        var noteResponseDto = _mappingService.Map<Note, NoteDto>(note);

        if (noteResponseDto is not null)
        {
            return DomainResult<NoteDto>.CreateSuccess(noteResponseDto, StatusCodes.Status200OK);
        }

        _logger.LogCritical(LogMessages.MappingErrorTemplate, typeof(Note), typeof(NoteDto));

        return DomainResult<NoteDto>.CreateFailure(Errors.InternalServerError, StatusCodes.Status500InternalServerError);
    }
}