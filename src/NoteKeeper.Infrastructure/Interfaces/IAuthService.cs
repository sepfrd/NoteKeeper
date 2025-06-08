using System.Linq.Expressions;
using NoteKeeper.Domain.Common;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Infrastructure.Common.Dtos;
using NoteKeeper.Infrastructure.Common.Dtos.Responses;

namespace NoteKeeper.Infrastructure.Interfaces;

public interface IAuthService
{
    Task<DomainResult<AuthResponseDto?>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

    Task<DomainResult<AuthResponseDto?>> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    string GetSignedInUserUuid();

    Task<User?> GetSignedInUserAsync(
        IEnumerable<Expression<Func<User, object?>>>? includes = null,
        CancellationToken cancellationToken = default);
}