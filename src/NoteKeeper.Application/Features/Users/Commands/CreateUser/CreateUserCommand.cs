using NoteKeeper.Application.Interfaces.CQRS;

namespace NoteKeeper.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string Username,
    string Password,
    string Email,
    string? FirstName,
    string? LastName) : ICommand;