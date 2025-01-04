using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NoteKeeper.DataAccess.Entities;
using NoteKeeper.DataAccess.Interfaces;

namespace NoteKeeper.DataAccess.Repositories;

public class RepositoryBase<T> : IRepositoryBase<T>
    where T : DomainEntity
{
    private readonly NoteKeeperDbContext _noteKeeperDbContext;
    private readonly DbSet<T> _dbSet;

    public RepositoryBase(NoteKeeperDbContext noteKeeperDbContext)
    {
        _noteKeeperDbContext = noteKeeperDbContext;
        _dbSet = noteKeeperDbContext.Set<T>();
    }

    public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entityEntry = await _dbSet.AddAsync(entity, cancellationToken);

        return entityEntry.Entity;
    }

    public async Task<List<T>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Expression<Func<T, bool>>? filterExpression = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object?>>? includeExpression = null,
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

        var sortedQuery = query.OrderByDescending(entity => entity.Id);

        var skipCount = (pageNumber - 1) * pageSize;

        var paginatedEntities = await sortedQuery.Skip(skipCount).Take(pageSize).ToListAsync(cancellationToken);

        return paginatedEntities;
    }

    public async Task<T?> GetByIdAsync(
        long id,
        Func<IQueryable<T>, IIncludableQueryable<T, object?>>? include = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(entity => entity.Id == id);

        if (include is not null)
        {
            query = include(query);
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<T?> GetByUuidAsync(
        Guid uuid,
        Func<IQueryable<T>, IIncludableQueryable<T, object?>>? include = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(entity => entity.Uuid == uuid);

        if (include is not null)
        {
            query = include(query);
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public T Delete(T entity) =>
        _dbSet.Remove(entity).Entity;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _noteKeeperDbContext.SaveChangesAsync(cancellationToken);
}