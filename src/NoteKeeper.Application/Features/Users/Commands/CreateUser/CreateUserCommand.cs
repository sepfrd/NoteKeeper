using NoteKeeper.Application.Interfaces.CQRS;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Shared.Utilities;

namespace NoteKeeper.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommand : ICommand
{
    public required string Username { get; init; }

    public required string Password { get; init; }

    public required string Email { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }
}