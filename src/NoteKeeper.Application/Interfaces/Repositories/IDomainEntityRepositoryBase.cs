using NoteKeeper.Domain.Entities;

namespace NoteKeeper.Application.Interfaces.Repositories;

public interface IDomainEntityRepositoryBase<TEntity> : IRepositoryBase<TEntity, long>
    where TEntity : DomainEntity
{
    Task<TEntity?> GetByIdentityAsync(Guid identity, CancellationToken cancellationToken = default);
}