using NoteKeeper.Application.Interfaces.Repositories;

namespace NoteKeeper.Application.Interfaces;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }

    INoteRepository NoteRepository { get; }

    IExternalProviderAccountRepository ExternalProviderAccountRepository { get; }

    int CommitChanges();

    Task<int> CommitChangesAsync(CancellationToken cancellationToken = default);
}