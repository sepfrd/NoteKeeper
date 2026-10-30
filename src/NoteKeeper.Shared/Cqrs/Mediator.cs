using Microsoft.Extensions.DependencyInjection;
using NoteKeeper.Shared.Cqrs.Commands;
using NoteKeeper.Shared.Cqrs.Notifications;
using NoteKeeper.Shared.Cqrs.Queries;

namespace NoteKeeper.Shared.Cqrs;

public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException(string.Format(
                Messages.NoQueryHandlerFoundErrorTemplate,
                query.GetType().Name));
        }

        return await (Task<TResult>)handlerType
            .GetMethod(nameof(IQueryHandler<,>.HandleAsync))!
            .Invoke(handler, [query, cancellationToken])!;
    }

    public async Task PublishAsync<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(notification, cancellationToken);
        }
    }

    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));

        var handler = _serviceProvider.GetRequiredService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException(string.Format(
                Messages.NoQueryHandlerFoundErrorTemplate,
                command.GetType().Name));
        }

        return await (Task<TResult>)handlerType
            .GetMethod(nameof(ICommandHandler<,>.HandleAsync))!
            .Invoke(handler, [command, cancellationToken])!;
    }
}