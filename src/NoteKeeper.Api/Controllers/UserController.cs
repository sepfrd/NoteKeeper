using FluentValidation;
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
    private readonly IValidator<CreateUserCommand> _validator;
    private readonly ICommandHandler<CreateUserCommand, DomainResult<UserDto>> _createUserCommandHandler;

    public UserController(ICommandHandler<CreateUserCommand, DomainResult<UserDto>> createUserCommandHandler, IValidator<CreateUserCommand> validator)
    {
        _createUserCommandHandler = createUserCommandHandler;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(DomainResult.CreateBaseFailure(validationResult.ToString(), StatusCodes.Status400BadRequest));
        }

        var result = await _createUserCommandHandler.HandleAsync(command, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }
}