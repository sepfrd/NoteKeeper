using System.Linq.Expressions;
using NoteKeeper.Domain.Common;
using NoteKeeper.Domain.Entities;

namespace NoteKeeper.Application.Interfaces.Repositories;

public interface IRepositoryBase<TEntity, in TKey>
    where TEntity : DomainEntity
    where TKey : IEquatable<TKey>
{
    Task<TEntity?> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task CreateManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<TEntity?> GetOneAsync(
        Expression<Func<TEntity, bool>> filter,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetOneAsync<TSorter>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TSorter>> orderBy,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        bool ascendingOrder = true,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default)
        where TSorter : IComparable<TSorter>;

    Task<TResult?> GetOneAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> subsetSelector,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default);

    Task<TResult?> GetOneAsync<TResult, TSorter>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> subsetSelector,
        Expression<Func<TEntity, TSorter>> orderBy,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        bool ascendingOrder = true,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default)
        where TSorter : IComparable<TSorter>;

    Task<PaginatedDomainResult<IEnumerable<TEntity>>> GetAllAsync(
        Expression<Func<TEntity, bool>> filter,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        uint page = 1,
        uint limit = 10,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default);

    Task<PaginatedDomainResult<IEnumerable<TEntity>>> GetAllAsync(
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        uint page = 1,
        uint limit = 10,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default);

    Task<PaginatedDomainResult<IEnumerable<TEntity>>> GetAllAsync<TSorter>(
        Expression<Func<TEntity, TSorter>> orderBy,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        uint page = 1,
        uint limit = 10,
        bool ascendingOrder = true,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default);

    Task<PaginatedDomainResult<IEnumerable<TEntity>>> GetAllAsync<TSorter>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TSorter>> orderBy,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        uint page = 1,
        uint limit = 10,
        bool ascendingOrder = true,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TResult>> GetAllAsync<TResult, TSorter>(
        Expression<Func<TEntity, TResult>> subsetSelector,
        Expression<Func<TEntity, TSorter>> orderBy,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        uint page = 1,
        uint limit = 10,
        bool ascendingOrder = true,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TResult>> GetAllAsync<TResult, TSorter>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> subsetSelector,
        Expression<Func<TEntity, TSorter>> orderBy,
        IEnumerable<Expression<Func<TEntity, object?>>>? includes = null,
        uint page = 1,
        uint limit = 10,
        bool ascendingOrder = true,
        bool useSplitQuery = false,
        bool disableTracking = false,
        CancellationToken cancellationToken = default);

    TEntity? Update(TEntity entityToUpdate);

    void Delete(TEntity entityToDelete);

    Task<long> GetCountAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default);
}