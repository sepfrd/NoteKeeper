using NoteKeeper.Business.Dtos;
using NoteKeeper.Business.Dtos.DomainEntities;
using NoteKeeper.Business.Dtos.Requests;

namespace NoteKeeper.Business.Interfaces;

public interface INoteService
{
    Task<ResponseDto<NoteDto?>> CreateAsync(CreateNoteRequestDto createNoteRequestDto, CancellationToken cancellationToken = default);

    Task<ResponseDto<List<NoteDto>>> GetAllAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ResponseDto<NoteDto?>> GetByUuidAsync(Guid noteUuid, CancellationToken cancellationToken = default);

    Task<ResponseDto<NoteDto?>> UpdateByUuidAsync(Guid noteUuid, UpdateNoteRequestDto updateNoteRequestDto, CancellationToken cancellationToken = default);

    Task<ResponseDto<NoteDto?>> DeleteByUuidAsync(Guid noteUuid, CancellationToken cancellationToken = default);

    Task SubscribeToNoteChangesAsync(Guid noteUuid);

    Task UnsubscribeFromNoteChangesAsync(Guid noteUuid);
}