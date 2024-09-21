using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using NoteKeeper.Business.Interfaces;

namespace NoteKeeper.Presentation.Authentication;

public class Ed25519JwtAuthenticationHandler : AuthenticationHandler<Ed25519JwtAuthenticationSchemeOptions>
{
    private readonly IAuthService _authService;

    public Ed25519JwtAuthenticationHandler(
        IOptionsMonitor<Ed25519JwtAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IAuthService authService)
        : base(options, logger, encoder)
    {
        _authService = authService;
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

        var isTokenValid = _authService.IsEd25519JwtValid(token);

        if (!isTokenValid)
        {
            return Task.FromResult(
                AuthenticateResult
                    .Fail(StatusCodes.Status401Unauthorized
                        .ToString(CultureInfo.InvariantCulture)));
        }

        var claimsPrincipal = _authService.ConvertJwtStringToClaimsPrincipal(token, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
    }
}