using Microsoft.AspNetCore.Mvc;
using NoteKeeper.Application.Features.Users.Commands.CreateUser;
using NoteKeeper.Application.Features.Users.Dtos;
using NoteKeeper.Application.Interfaces.CQRS;
using NoteKeeper.Domain.Common;

namespace NoteKeeper.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly ICommandHandler<CreateUserCommand, DomainResult<UserDto>> _createUserCommandHandler;

    public UserController(ICommandHandler<CreateUserCommand, DomainResult<UserDto>> createUserCommandHandler)
    {
        _createUserCommandHandler = createUserCommandHandler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _createUserCommandHandler.HandleAsync(command, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }
}