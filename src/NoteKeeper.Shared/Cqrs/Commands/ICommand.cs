namespace NoteKeeper.Shared.Cqrs.Commands;

public interface ICommand;

public interface ICommand<TResponse> : ICommand;