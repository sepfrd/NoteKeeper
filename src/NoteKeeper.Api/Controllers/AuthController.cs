using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NoteKeeper.Api.Constants;
using NoteKeeper.Domain.Common;
using NoteKeeper.Infrastructure.Common.Dtos.Requests;
using NoteKeeper.Infrastructure.Interfaces;
using NoteKeeper.Shared.Resources;

namespace NoteKeeper.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<LoginRequestDto> _validator;

    public AuthController(IAuthService authService, IValidator<LoginRequestDto> validator)
    {
        _authService = authService;
        _validator = validator;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto loginRequestDto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(loginRequestDto, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(DomainResult.CreateBaseFailure(validationResult.ToString(), StatusCodes.Status400BadRequest));
        }

        var result = await _authService.LoginAsync(loginRequestDto, cancellationToken);

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
    [Authorize]
    [Route("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(KeyConstants.RefreshTokenCookieKey, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });

        return Ok(DomainResult.CreateBaseSuccess(SuccessMessages.Logout, StatusCodes.Status200OK));
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