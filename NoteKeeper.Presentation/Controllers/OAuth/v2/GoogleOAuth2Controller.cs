using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NoteKeeper.Business.Dtos.Google;
using NoteKeeper.Business.Interfaces;

namespace NoteKeeper.Presentation.Controllers.OAuth.v2;

[ApiController]
[Route("api/oauth/v2/google")]
public class GoogleOAuth2Controller : ControllerBase
{
    private readonly IGoogleOAuth2Service _googleOAuth2Service;

    public GoogleOAuth2Controller(IGoogleOAuth2Service googleOAuth2Service) =>
        _googleOAuth2Service = googleOAuth2Service;

    [HttpPost]
    [Route("oidc")]
    public async Task<IActionResult> AuthenticateWithGoogleAsync(CancellationToken cancellationToken)
    {
        var result = await _googleOAuth2Service.AuthenticateWithGoogleAsync(cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet]
    [Route("code")]
    public async Task<IActionResult> ReceiveGoogleAuthorizationCode(
        [FromQuery] string state,
        [FromQuery] string code,
        [FromQuery] string scope,
        [FromQuery] string authuser,
        [FromQuery] string prompt,
        CancellationToken cancellationToken)
    {
        var exchangeRequestDto = new CompleteGoogleAuthenticationAsyncRequestDto(state, code, scope, authuser, prompt);

        var result = await _googleOAuth2Service.CompleteGoogleAuthenticationAsync(exchangeRequestDto, cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPatch]
    [Authorize]
    [Route("google/tokens")]
    public async Task<IActionResult> RefreshGoogleTokensAsync(CancellationToken cancellationToken)
    {
        var result = await _googleOAuth2Service.GoogleRefreshAccessTokenAsync(cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete]
    [Authorize]
    [Route("google/tokens")]
    public async Task<IActionResult> RevokeGoogleTokensAsync(CancellationToken cancellationToken)
    {
        var result = await _googleOAuth2Service.RevokeTokensAsync(cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpOptions]
    public IActionResult GoogleOAuth2Options()
    {
        Response
            .Headers
            .Add(new KeyValuePair<string, StringValues>("Allow", $"{HttpMethods.Post},{HttpMethods.Get}"));

        return Ok();
    }
}