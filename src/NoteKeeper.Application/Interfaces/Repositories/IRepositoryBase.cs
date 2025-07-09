using System.Linq.Expressions;
using NoteKeeper.Domain.Common;
using NoteKeeper.Domain.Interfaces;

namespace NoteKeeper.Application.Interfaces.Repositories;

public interface IRepositoryBase<TEntity, in TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    Task<TEntity?> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdentityAsync(TKey identity, CancellationToken cancellationToken = default);

    Task<TEntity?> GetOneAsync(
        IFilterBase<TEntity> filter,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default);

    Task<PaginatedDomainResult<IEnumerable<TEntity>>> GetAllAsync(
        IFilterBase<TEntity>? filter = null,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        uint page = 1,
        uint limit = 10,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetOneAsync(
        Expression<Func<TEntity, bool>> filter,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default);

    Task<PaginatedDomainResult<IEnumerable<TEntity>>> GetAllAsync(
        Expression<Func<TEntity, bool>> filter,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        uint page = 1,
        uint limit = 10,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default);

    TEntity Update(TEntity entityToUpdate);

    void Delete(TEntity entityToDelete);

    Task<long> GetCountAsync(IFilterBase<TEntity>? filter = null, CancellationToken cancellationToken = default);
}