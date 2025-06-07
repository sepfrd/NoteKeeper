using NoteKeeper.Domain.Common;
using NoteKeeper.Infrastructure.Common.Dtos;
using NoteKeeper.Infrastructure.Common.Dtos.Responses;

namespace NoteKeeper.Infrastructure.Interfaces;

public interface IAuthService
{
    Task<DomainResult<AuthResponseDto?>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

    Task<DomainResult<AuthResponseDto?>> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    string GetSignedInUserUuid();
}