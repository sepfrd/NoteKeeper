using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NoteKeeper.Application.Features.Notes.Dtos;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Application.Interfaces.CQRS;
using NoteKeeper.Domain.Common;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Shared.Resources;

namespace NoteKeeper.Application.Features.Notes.Queries.GetNoteByUuid;

public class GetNoteByUuidQueryHandler : IQueryHandler<GetNoteByUuidQuery, DomainResult<NoteDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMappingService _mappingService;
    private readonly ILogger<GetNoteByUuidQueryHandler> _logger;

    public GetNoteByUuidQueryHandler(
        IUnitOfWork unitOfWork,
        IMappingService mappingService,
        ILogger<GetNoteByUuidQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mappingService = mappingService;
        _logger = logger;
    }

    public async Task<DomainResult<NoteDto>> HandleAsync(GetNoteByUuidQuery query, CancellationToken cancellationToken = default)
    {
        var note = await _unitOfWork.NoteRepository.GetOneAsync(
            filter: note => note.Uuid == query.NoteUuid,
            includes: [note => note.User],
            disableTracking: true,
            cancellationToken: cancellationToken);

        if (note is null)
        {
            var notFoundMessage = string.Format(
                ErrorMessages.EntityNotFoundByUuidTemplate,
                nameof(Note).Humanize(LetterCasing.LowerCase),
                query.NoteUuid);

            return DomainResult<NoteDto>.CreateFailure(ErrorMessages.EntityNotFoundByUuidTemplate, StatusCodes.Status404NotFound);
        }

        var noteResponseDto = _mappingService.Map<Note, NoteDto>(note);

        if (noteResponseDto is not null)
        {
            return DomainResult<NoteDto>.CreateSuccess(null, StatusCodes.Status200OK, noteResponseDto);
        }

        _logger.LogCritical(LogMessages.MappingErrorTemplate, typeof(Note), typeof(NoteDto));

        return DomainResult<NoteDto>.CreateFailure(
            ErrorMessages.InternalServerError,
            StatusCodes.Status500InternalServerError);
    }
}