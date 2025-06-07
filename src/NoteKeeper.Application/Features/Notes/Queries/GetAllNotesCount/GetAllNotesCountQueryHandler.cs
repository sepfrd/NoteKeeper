using Microsoft.AspNetCore.Http;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Application.Interfaces.CQRS;
using NoteKeeper.Domain.Common;

namespace NoteKeeper.Application.Features.Notes.Queries.GetAllNotesCount;

public class GetAllNotesCountQueryHandler : IQueryHandler<GetAllNotesCountQuery, DomainResult<long>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllNotesCountQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DomainResult<long>> HandleAsync(GetAllNotesCountQuery query, CancellationToken cancellationToken)
    {
        var count = await _unitOfWork.NoteRepository.GetCountAsync(null, cancellationToken);

        return DomainResult<long>.CreateSuccess(null, StatusCodes.Status200OK, count);
    }
}