using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using NoteKeeper.Infrastructure.Interfaces;

namespace NoteKeeper.Api.Authentication;

public class Ed25519JwtAuthenticationHandler : AuthenticationHandler<Ed25519JwtAuthenticationSchemeOptions>
{
    private readonly ITokenService _tokenService;

    public Ed25519JwtAuthenticationHandler(
        IOptionsMonitor<Ed25519JwtAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ITokenService tokenService)
        : base(options, logger, encoder)
    {
        _tokenService = tokenService;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return Task.FromResult(
                AuthenticateResult
                    .Fail(StatusCodes.Status401Unauthorized
                        .ToString(CultureInfo.InvariantCulture)));
        }

        var token = authorizationHeader.Contains(JwtBearerDefaults.AuthenticationScheme)
            ? authorizationHeader.Replace(JwtBearerDefaults.AuthenticationScheme, string.Empty).Trim()
            : authorizationHeader.Trim();

        var isTokenValid = _tokenService.ValidateEd25519Jwt(token, out var claims);

        if (!isTokenValid || claims is null)
        {
            return Task.FromResult(
                AuthenticateResult
                    .Fail(StatusCodes.Status401Unauthorized
                        .ToString(CultureInfo.InvariantCulture)));
        }

        var claimsIdentity = new ClaimsIdentity(
            claims.Select(claim => new Claim(claim.Key, claim.Value.ToString() ?? string.Empty)),
            Scheme.Name);

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
    }
}