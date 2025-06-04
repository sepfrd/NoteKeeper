using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NoteKeeper.Business.Dtos;
using NoteKeeper.Business.Interfaces;

namespace NoteKeeper.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieKey = "refresh_token";
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
            Response.Cookies.Append(RefreshTokenCookieKey, result.Data!.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = result.Data.RefreshTokenExpiresAt
            });
        }

        return StatusCode((int)result.HttpStatusCode, new ResponseDto<string?>
        {
            Data = result.Data?.Jwt,
            IsSuccess = result.IsSuccess,
            Message = result.Message,
            HttpStatusCode = result.HttpStatusCode
        });
    }

    [HttpPost]
    [Route("refresh-token")]
    public async Task<IActionResult> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookieKey, out var refreshToken))
        {
            return Unauthorized();
        }

        var result = await _authService.RefreshAccessTokenAsync(refreshToken, cancellationToken);

        if (result.IsSuccess)
        {
            Response.Cookies.Append(RefreshTokenCookieKey, result.Data!.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = result.Data.RefreshTokenExpiresAt
            });
        }

        return StatusCode((int)result.HttpStatusCode, new ResponseDto<string?>
        {
            Data = result.Data?.Jwt,
            IsSuccess = result.IsSuccess,
            Message = result.Message,
            HttpStatusCode = result.HttpStatusCode
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