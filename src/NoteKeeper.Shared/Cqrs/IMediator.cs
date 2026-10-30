using NoteKeeper.Shared.Cqrs.Commands;
using NoteKeeper.Shared.Cqrs.Notifications;
using NoteKeeper.Shared.Cqrs.Queries;

namespace NoteKeeper.Shared.Cqrs;

public interface IMediator
{
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    Task<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);

    Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}