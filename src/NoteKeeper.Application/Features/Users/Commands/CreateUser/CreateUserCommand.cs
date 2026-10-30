using NoteKeeper.Application.Features.Users.Dtos;
using NoteKeeper.Shared.Cqrs.Commands;
using NoteKeeper.Shared.Results;

namespace NoteKeeper.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string Username,
    string Password,
    string Email,
    string? FirstName,
    string? LastName) : ICommand<DomainResult<UserDto>>;