using Microsoft.AspNetCore.Mvc;
using NoteKeeper.Business.Dtos.Requests;
using NoteKeeper.Business.Interfaces;

namespace NoteKeeper.Presentation.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserRequestDto createUserRequestDto, CancellationToken cancellationToken)
    {
        var result = await _userService.CreateUserAsync(createUserRequestDto, cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }
}