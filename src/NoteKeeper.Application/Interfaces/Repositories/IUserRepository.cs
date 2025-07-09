using NoteKeeper.Domain.Entities;

namespace NoteKeeper.Application.Interfaces.Repositories;

public interface IUserRepository : IDomainEntityRepositoryBase<User>
{
    Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default);

    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
}