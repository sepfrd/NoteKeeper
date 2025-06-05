using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NoteKeeper.DataAccess.Common;
using NoteKeeper.DataAccess.Entities;
using NoteKeeper.DataAccess.Interfaces;

namespace NoteKeeper.DataAccess.Repositories;

public class RepositoryBase<TEntity> : IRepositoryBase<TEntity>
    where TEntity : DomainEntity
{
    private readonly NoteKeeperDbContext _noteKeeperDbContext;
    private readonly DbSet<TEntity> _dbSet;

    public RepositoryBase(NoteKeeperDbContext noteKeeperDbContext)
    {
        _noteKeeperDbContext = noteKeeperDbContext;
        _dbSet = noteKeeperDbContext.Set<TEntity>();
    }

    public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entityEntry = await _dbSet.AddAsync(entity, cancellationToken);

        return entityEntry.Entity;
    }

    public async Task<long> GetCountAsync(Expression<Func<TEntity, bool>>? filterExpression = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();

        if (filterExpression is not null)
        {
            query = query.Where(filterExpression);
        }

        return await query.LongCountAsync(cancellationToken);
    }

    public async Task<PaginatedResult<TEntity>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Expression<Func<TEntity, bool>>? filterExpression = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object?>>? includeExpression = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();

        if (filterExpression is not null)
        {
            query = query.Where(filterExpression);
        }

        if (includeExpression is not null)
        {
            query = includeExpression(query);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var sortedQuery = query.OrderByDescending(entity => entity.Id);

        var skipCount = (pageNumber - 1) * pageSize;

        var paginatedEntities = sortedQuery.Skip(skipCount).Take(pageSize);

        return new PaginatedResult<TEntity>(
            pageNumber,
            pageSize,
            totalCount,
            paginatedEntities);
    }

    public async Task<TEntity?> GetByIdAsync(
        long id,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object?>>? include = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(entity => entity.Id == id);

        if (include is not null)
        {
            query = include(query);
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByUuidAsync(
        Guid uuid,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object?>>? include = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(entity => entity.Uuid == uuid);

        if (include is not null)
        {
            query = include(query);
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public TEntity Delete(TEntity entity) =>
        _dbSet.Remove(entity).Entity;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _noteKeeperDbContext.SaveChangesAsync(cancellationToken);
}