using NoteKeeper.Business.Dtos;
using NoteKeeper.Business.Dtos.DomainEntities;
using NoteKeeper.Business.Dtos.Requests;

namespace NoteKeeper.Business.Interfaces;

public interface IUserService
{
    Task<ResponseDto<UserDto?>> CreateUserAsync(CreateUserRequestDto createUserRequestDto, CancellationToken cancellationToken = default);
}