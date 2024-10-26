using Microsoft.EntityFrameworkCore.Query;
using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.DataAccess.Interfaces;

public interface IUserRepository : IRepositoryBase<User>
{
    Task<User?> GetByUsernameAsync(
        string username,
        Func<IQueryable<User>, IIncludableQueryable<User, object?>>? include = null,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        string email,
        Func<IQueryable<User>, IIncludableQueryable<User, object?>>? include = null,
        CancellationToken cancellationToken = default);

    Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default);

    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
}