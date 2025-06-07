using NoteKeeper.Application.Interfaces.CQRS;

namespace NoteKeeper.Application.Features.Notes.Commands.CreateNote;

public record CreateNoteCommand(string Title, string Content, Guid UserUuid) : ICommand;