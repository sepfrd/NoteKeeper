using NoteKeeper.Application.Interfaces;
using NoteKeeper.Application.Interfaces.Repositories;

namespace InsightFlow.Application.Interfaces;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }

    INoteRepository NoteRepository { get; }

    int CommitChanges();

    Task<int> CommitChangesAsync(CancellationToken cancellationToken = default);
}