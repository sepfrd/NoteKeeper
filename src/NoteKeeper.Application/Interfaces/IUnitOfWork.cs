using NoteKeeper.Application.Interfaces.Repositories;

namespace NoteKeeper.Application.Interfaces;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }

    INoteRepository NoteRepository { get; }

    INotionTokenRepository NotionTokenRepository { get; }

    IGoogleTokenRepository GoogleTokenRepository { get; }

    int CommitChanges();

    Task<int> CommitChangesAsync(CancellationToken cancellationToken = default);
}