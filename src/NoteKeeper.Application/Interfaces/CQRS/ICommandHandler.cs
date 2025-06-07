using NoteKeeper.Domain.Common;

namespace NoteKeeper.Application.Interfaces.CQRS;

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand
    where TResult : DomainResult
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}