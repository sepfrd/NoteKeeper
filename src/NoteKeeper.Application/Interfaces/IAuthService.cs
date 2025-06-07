using NoteKeeper.Domain.Entities;

namespace NoteKeeper.Application.Interfaces;

public interface IAuthService
{
    Task<ResponseDto<AuthResponseDto?>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

    Task<ResponseDto<AuthResponseDto?>> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<User?> GetSignedInUserAsync(Func<IQueryable<User>, IIncludableQueryable<User, object?>>? include = null, CancellationToken cancellationToken = default);
}