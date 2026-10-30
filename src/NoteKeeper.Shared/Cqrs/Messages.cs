namespace NoteKeeper.Shared.Cqrs;

public static class Messages
{
    public const string NoQueryHandlerFoundErrorTemplate = "No handler found for query of type {0}";
    public const string NoCommandHandlerFoundErrorTemplate = "No handler found for command of type {0}";
}