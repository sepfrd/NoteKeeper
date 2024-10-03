using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NoteKeeper.Business.Dtos;
using NoteKeeper.Business.Interfaces;

namespace NoteKeeper.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) =>
        _authService = authService;

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(loginDto, cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpOptions]
    public IActionResult AuthOptions()
    {
        Response
            .Headers
            .Add(new KeyValuePair<string, StringValues>("Allow", $"{HttpMethods.Post}"));

        return Ok();
    }

    [HttpGet]
    [Authorize]
    [Route("oauth2/google")]
    public async Task<IActionResult> GetGoogleOAuth2RequestUrl(CancellationToken cancellationToken)
    {
        var result = await _authService.BuildGoogleOAuth2RequestUrlAsync(cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet]
    [Route("oauth2/google/code")]
    public async Task<IActionResult> ReceiveAuthorizationCode(
        [FromQuery] string state,
        [FromQuery] string code,
        [FromQuery] string scope,
        [FromQuery] string authuser,
        [FromQuery] string prompt,
        CancellationToken cancellationToken)
    {
        var exchangeRequestDto = new GoogleExchangeCodeForTokenRequestDto(state, code, scope, authuser, prompt);

        var result = await _authService.GoogleExchangeCodeForTokenAsync(exchangeRequestDto, cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }
}