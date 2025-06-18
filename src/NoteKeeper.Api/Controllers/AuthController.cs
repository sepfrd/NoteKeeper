using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NoteKeeper.Api.Constants;
using NoteKeeper.Infrastructure.Common.Dtos;
using NoteKeeper.Infrastructure.Interfaces;

namespace NoteKeeper.Api.Controllers;

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

        if (result.IsSuccess)
        {
            Response.Cookies.Append(KeyConstants.RefreshTokenCookieKey, result.Data!.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = result.Data.RefreshTokenExpiresAt
            });
        }

        return StatusCode(result.StatusCode, new
        {
            Data = result.Data?.Jwt,
            result.IsSuccess,
            result.Message,
            result.StatusCode
        });
    }

    [HttpPost]
    [Route("refresh-token")]
    public async Task<IActionResult> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(KeyConstants.RefreshTokenCookieKey, out var refreshToken))
        {
            return Unauthorized();
        }

        var result = await _authService.RefreshAccessTokenAsync(refreshToken, cancellationToken);

        if (result.IsSuccess)
        {
            Response.Cookies.Append(KeyConstants.RefreshTokenCookieKey, result.Data!.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = result.Data.RefreshTokenExpiresAt
            });
        }
        else
        {
            Response.Cookies.Delete(KeyConstants.RefreshTokenCookieKey, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
        }

        return StatusCode(result.StatusCode, new
        {
            Data = result.Data?.Jwt,
            result.IsSuccess,
            result.Message,
            result.StatusCode
        });
    }

    [HttpOptions]
    public IActionResult AuthOptions()
    {
        Response
            .Headers
            .Add(new KeyValuePair<string, StringValues>("Allow", $"{HttpMethods.Post}"));

        return Ok();
    }
}