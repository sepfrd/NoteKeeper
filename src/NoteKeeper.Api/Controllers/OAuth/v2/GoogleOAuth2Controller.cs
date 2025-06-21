using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NoteKeeper.Api.Constants;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google.Models;
using NoteKeeper.Infrastructure.Interfaces;

namespace NoteKeeper.Api.Controllers.OAuth.v2;

[ApiController]
[Route("api/oauth/v2/google")]
public class GoogleOAuth2Controller : ControllerBase
{
    private readonly IGoogleOAuth2Service _googleOAuth2Service;

    public GoogleOAuth2Controller(IGoogleOAuth2Service googleOAuth2Service) =>
        _googleOAuth2Service = googleOAuth2Service;

    [HttpPost]
    [Route("oidc")]
    public async Task<IActionResult> AuthenticateWithGoogleAsync([FromQuery] string redirectUri, CancellationToken cancellationToken)
    {
        var result = await _googleOAuth2Service.AuthenticateWithGoogleAsync(redirectUri, cancellationToken);

        return StatusCode(result.StatusCode, result);
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
        var exchangeRequestDto = new CompleteGoogleAuthenticationRequestDto(state, code, scope, authuser, prompt);

        var result = await _googleOAuth2Service.CompleteGoogleAuthenticationAsync(exchangeRequestDto, cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new
            {
                Data = result.Data?.AuthResponseDto.Jwt,
                result.IsSuccess,
                result.Message,
                result.StatusCode
            });
        }

        Response.Cookies.Append(KeyConstants.RefreshTokenCookieKey, result.Data!.AuthResponseDto.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = result.Data.AuthResponseDto.RefreshTokenExpiresAt
        });

        var redirectUri = $"{result.Data.RedirectUri.TrimEnd('/')}?{KeyConstants.AccessTokenQueryParameterKey}={result.Data.AuthResponseDto.Jwt}";

        return Redirect(redirectUri);
    }

    [HttpPatch]
    [Authorize]
    [Route("google/tokens")]
    public async Task<IActionResult> RefreshGoogleTokensAsync(CancellationToken cancellationToken)
    {
        var result = await _googleOAuth2Service.GoogleRefreshAccessTokenAsync(cancellationToken);

        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete]
    [Authorize]
    [Route("google/tokens")]
    public async Task<IActionResult> RevokeGoogleTokensAsync(CancellationToken cancellationToken)
    {
        var result = await _googleOAuth2Service.RevokeTokensAsync(cancellationToken);

        return StatusCode(result.StatusCode, result);
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