using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NoteKeeper.Business.Dtos;
using NoteKeeper.Business.Interfaces;

namespace NoteKeeper.Presentation.Controllers;

[ApiController]
[Route("api/oauth/v2")]
public class OAuth2Controller : ControllerBase
{
    private readonly IGoogleOAuth2Service _googleOAuth2Service;

    public OAuth2Controller(IGoogleOAuth2Service googleOAuth2Service) =>
        _googleOAuth2Service = googleOAuth2Service;

    [HttpPost]
    [Route("google/connect")]
    public async Task<IActionResult> UseGoogleOAuth2Async(CancellationToken cancellationToken)
    {
        var result = await _googleOAuth2Service.UseGoogleOAuth2Async(cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet]
    [Route("google/code")]
    public async Task<IActionResult> ReceiveAuthorizationCode(
        [FromQuery] string state,
        [FromQuery] string code,
        [FromQuery] string scope,
        [FromQuery] string authuser,
        [FromQuery] string prompt,
        CancellationToken cancellationToken)
    {
        var exchangeRequestDto = new GoogleExchangeCodeForTokenRequestDto(state, code, scope, authuser, prompt);

        var result = await _googleOAuth2Service.GoogleExchangeCodeForTokensAsync(exchangeRequestDto, cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete]
    [Authorize]
    [Route("google/tokens")]
    public async Task<IActionResult> RevokeTokensAsync(CancellationToken cancellationToken)
    {
        var result = await _googleOAuth2Service.RevokeTokensAsync(cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpOptions]
    public IActionResult OAuth2Options()
    {
        Response
            .Headers
            .Add(new KeyValuePair<string, StringValues>("Allow", $"{HttpMethods.Post},{HttpMethods.Get}"));

        return Ok();
    }
}