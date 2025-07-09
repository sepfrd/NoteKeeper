using Microsoft.EntityFrameworkCore;
using NoteKeeper.Application.Interfaces.Repositories;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Infrastructure.Interfaces;

namespace NoteKeeper.Infrastructure.Persistence.Repositories;

public class UserRepository : DomainEntityRepositoryBase<User>, IUserRepository
{
    public UserRepository(DbSet<User> dbSet, IDbConnectionPool dbConnectionPool)
        : base(dbSet, dbConnectionPool)
    {
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _dbSet.FirstOrDefaultAsync(
            user => user.Email == email,
            cancellationToken);

        return user is null;
    }

    public async Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _dbSet.FirstOrDefaultAsync(
            user => user.Username == username,
            cancellationToken);

        return user is null;
    }
}