using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.DataAccess.Interfaces;

public interface IRepositoryBase<T> where T : DomainEntity
{
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    Task<List<T>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Expression<Func<T, bool>>? filterExpression = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object?>>? includeExpression = null,
        CancellationToken cancellationToken = default);

    Task<T?> GetByIdAsync(long id, Func<IQueryable<T>, IIncludableQueryable<T, object?>>? include = null, CancellationToken cancellationToken = default);

    T Delete(T entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}