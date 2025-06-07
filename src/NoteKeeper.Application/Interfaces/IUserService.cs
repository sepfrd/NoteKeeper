using NoteKeeper.Application.Features.Users.Dtos;

namespace NoteKeeper.Application.Interfaces;

public interface IUserService
{
    Task<ResponseDto<UserDto?>> CreateUserAsync(CreateUserRequestDto createUserRequestDto, CancellationToken cancellationToken = default);
}