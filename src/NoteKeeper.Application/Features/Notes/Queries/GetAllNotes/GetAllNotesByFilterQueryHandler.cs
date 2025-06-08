using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NoteKeeper.Application.Features.Notes.Dtos;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Application.Interfaces.CQRS;
using NoteKeeper.Domain.Common;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Shared.Resources;

namespace NoteKeeper.Application.Features.Notes.Queries.GetAllNotes;

public class GetAllNotesByFilterQueryHandler : IQueryHandler<GetAllNotesByFilterQuery, PaginatedDomainResult<IEnumerable<NoteDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMappingService _mappingService;
    private readonly ILogger<GetAllNotesByFilterQueryHandler> _logger;

    public GetAllNotesByFilterQueryHandler(
        IUnitOfWork unitOfWork,
        IMappingService mappingService,
        ILogger<GetAllNotesByFilterQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mappingService = mappingService;
        _logger = logger;
    }

    public async Task<PaginatedDomainResult<IEnumerable<NoteDto>>> HandleAsync(GetAllNotesByFilterQuery request, CancellationToken cancellationToken)
    {
        var filterExpression = request.FilterDto?.ToExpression() ?? (_ => true);

        var notesResponse = await _unitOfWork.NoteRepository.GetAllAsync(
            filter: filterExpression,
            includes: [note => note.User],
            page: request.PageNumber,
            limit: request.PageSize,
            cancellationToken: cancellationToken);

        var noteDtos = _mappingService.Map<IEnumerable<Note>, IEnumerable<NoteDto>>(notesResponse.Data);

        if (noteDtos is null)
        {
            _logger.LogCritical(LogMessages.MappingErrorTemplate, typeof(IEnumerable<Note>), typeof(IEnumerable<NoteDto>));

            return PaginatedDomainResult<IEnumerable<NoteDto>>.CreateFailure(
                ErrorMessages.InternalServerError,
                StatusCodes.Status500InternalServerError);
        }

        var response = PaginatedDomainResult<IEnumerable<NoteDto>>.CreateSuccess(
            null,
            StatusCodes.Status200OK,
            noteDtos,
            request.PageNumber,
            request.PageSize,
            notesResponse.TotalCount);

        return response;
    }
}