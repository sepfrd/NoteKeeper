using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using NoteKeeper.DataAccess.Common;
using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.DataAccess.Interfaces;

public interface IRepositoryBase<TEntity> where TEntity : DomainEntity
{
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(Expression<Func<TEntity, bool>>? filterExpression = null, CancellationToken cancellationToken = default);

    Task<PaginatedResult<TEntity>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Expression<Func<TEntity, bool>>? filterExpression = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object?>>? includeExpression = null,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(long id, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object?>>? include = null, CancellationToken cancellationToken = default);

    Task<TEntity?> GetByUuidAsync(Guid uuid, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object?>>? include = null, CancellationToken cancellationToken = default);

    TEntity Delete(TEntity entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}