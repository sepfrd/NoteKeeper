using Microsoft.EntityFrameworkCore;
using NoteKeeper.Application.Interfaces.Repositories;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Infrastructure.Interfaces;

namespace NoteKeeper.Infrastructure.Persistence.Repositories;

public class DomainEntityRepositoryBase<TEntity> : RepositoryBase<TEntity, long>, IDomainEntityRepositoryBase<TEntity>
    where TEntity : DomainEntity
{
    public DomainEntityRepositoryBase(DbSet<TEntity> dbSet, IDbConnectionPool dbConnectionPool)
        : base(dbSet, dbConnectionPool)
    {
    }

    public async Task<TEntity?> GetByIdentityAsync(Guid identity, CancellationToken cancellationToken = default) =>
        await _dbSet.SingleOrDefaultAsync(entity => entity.Uuid == identity, cancellationToken);
}