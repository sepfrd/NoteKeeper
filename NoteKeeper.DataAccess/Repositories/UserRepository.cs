using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NoteKeeper.DataAccess.Entities;
using NoteKeeper.DataAccess.Interfaces;

namespace NoteKeeper.DataAccess.Repositories;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    private readonly NoteKeeperDbContext _noteKeeperDbContext;

    public UserRepository(NoteKeeperDbContext noteKeeperDbContext) : base(noteKeeperDbContext)
    {
        _noteKeeperDbContext = noteKeeperDbContext;
    }

    public async Task<User?> GetByUsernameAsync(
        string username,
        Func<IQueryable<User>, IIncludableQueryable<User, object?>>? include = null,
        CancellationToken cancellationToken = default)
    {
        var query = _noteKeeperDbContext.Users.Where(user => user.Username == username);

        if (include is not null)
        {
            query = include(query);
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(
        string email,
        Func<IQueryable<User>, IIncludableQueryable<User, object?>>? include = null,
        CancellationToken cancellationToken = default)
    {
        var query = _noteKeeperDbContext.Users.Where(user => user.Email == email);

        if (include is not null)
        {
            query = include(query);
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default) =>
        !await _noteKeeperDbContext
            .Users
            .AnyAsync(user =>
                    string.Equals(username, user.Username, StringComparison.InvariantCultureIgnoreCase),
                cancellationToken);

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default) =>
        !await _noteKeeperDbContext
            .Users
            .AnyAsync(user =>
                    string.Equals(email, user.Email, StringComparison.InvariantCultureIgnoreCase),
                cancellationToken);
}