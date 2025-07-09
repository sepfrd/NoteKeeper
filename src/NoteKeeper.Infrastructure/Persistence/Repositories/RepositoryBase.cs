using System.Linq.Expressions;
using Dapper;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Application.Interfaces.Repositories;
using NoteKeeper.Domain.Common;
using NoteKeeper.Domain.Interfaces;
using NoteKeeper.Infrastructure.Common.Constants;
using NoteKeeper.Infrastructure.Interfaces;

namespace NoteKeeper.Infrastructure.Persistence.Repositories;

public abstract class RepositoryBase<TEntity, TKey> : IRepositoryBase<TEntity, TKey>
    where TEntity : class, IEntity<TKey>, IAuditable
    where TKey : IEquatable<TKey>
{
    private readonly string _tableName;
    protected readonly DbSet<TEntity> _dbSet;
    private readonly IDbConnectionPool _dbConnectionPool;

    protected RepositoryBase(DbSet<TEntity> dbSet, IDbConnectionPool dbConnectionPool)
    {
        _dbSet = dbSet;
        _dbConnectionPool = dbConnectionPool;
        _tableName = $"\"{_dbSet.EntityType.ShortName().Pluralize()}\"";
    }

    public virtual async Task<TEntity?> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entityEntry = await _dbSet
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        return entityEntry.Entity;
    }

    public async Task<TEntity?> GetByIdentityAsync(TKey identity, CancellationToken cancellationToken = default) =>
        await _dbSet.SingleOrDefaultAsync(entity => entity.Id.Equals(identity), cancellationToken);

    public virtual async Task<TEntity?> GetOneAsync(
        IFilterBase<TEntity> filter,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default)
    {
        var data = _dbSet.Where(filter.ToFilterExpression());

        if (includes is not null)
        {
            data = includes.Aggregate(data, (current, include) => current.Include(include));
        }

        if (useSplitQuery)
        {
            data = data.AsSplitQuery();
        }

        if (disableTracking)
        {
            data = data.AsNoTracking();
        }

        return await data.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<PaginatedDomainResult<IEnumerable<TEntity>>> GetAllAsync(
        IFilterBase<TEntity>? filter,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        uint page = 1,
        uint limit = 10,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default)
    {
        var data = filter is not null
            ? _dbSet.Where(filter.ToFilterExpression())
            : _dbSet;

        if (includes is not null)
        {
            data = includes.Aggregate(data, (current, include) => current.Include(include));
        }

        var totalCount = await data.CountAsync(cancellationToken).ConfigureAwait(false);

        data = data
            .OrderBy(entity => entity.Id)
            .Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(limit))
            .Take(Convert.ToInt32(limit));

        if (useSplitQuery)
        {
            data = data.AsSplitQuery();
        }

        if (disableTracking)
        {
            data = data.AsNoTracking();
        }

        var response = PaginatedDomainResult<IEnumerable<TEntity>>.CreateSuccess(
            null,
            StatusCodes.Status200OK,
            data,
            page,
            limit,
            (uint)totalCount);

        return response;
    }

    public virtual async Task<TEntity?> GetOneAsync(
        Expression<Func<TEntity, bool>> filter,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default)
    {
        var data = _dbSet.Where(filter);

        if (includes is not null)
        {
            data = includes.Aggregate(data, (current, include) => current.Include(include));
        }

        if (useSplitQuery)
        {
            data = data.AsSplitQuery();
        }

        if (disableTracking)
        {
            data = data.AsNoTracking();
        }

        return await data.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<PaginatedDomainResult<IEnumerable<TEntity>>> GetAllAsync(
        Expression<Func<TEntity, bool>> filter,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        uint page = 1,
        uint limit = 10,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default)
    {
        var data = _dbSet.Where(filter);

        if (includes is not null)
        {
            data = includes.Aggregate(data, (current, include) => current.Include(include));
        }

        var totalCount = await data.CountAsync(cancellationToken).ConfigureAwait(false);

        data = data
            .OrderBy(entity => entity.Id)
            .Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(limit))
            .Take(Convert.ToInt32(limit));

        if (useSplitQuery)
        {
            data = data.AsSplitQuery();
        }

        if (disableTracking)
        {
            data = data.AsNoTracking();
        }

        var response = PaginatedDomainResult<IEnumerable<TEntity>>.CreateSuccess(
            null,
            StatusCodes.Status200OK,
            data,
            page,
            limit,
            (uint)totalCount);

        return response;
    }

    public virtual async Task<long> GetCountAsync(IFilterBase<TEntity>? filter = null, CancellationToken cancellationToken = default)
    {
        var connection = _dbConnectionPool.GetConnection();

        var sqlQuery = string.Format(SqlQueryConstants.GetAllCountQuery, _tableName);

        var count = await connection.ExecuteScalarAsync<long>(sqlQuery);

        _dbConnectionPool.ReturnConnection(connection);

        return count;
    }

    public TEntity Update(TEntity entityToUpdate)
    {
        entityToUpdate.MarkAsUpdated();

        return _dbSet
            .Update(entityToUpdate)
            .Entity;
    }

    public void Delete(TEntity entityToDelete) =>
        _dbSet.Remove(entityToDelete);
}