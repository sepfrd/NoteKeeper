using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

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

        var isTokenValid = _tokenService.IsEd25519JwtValid(token);

        if (!isTokenValid)
        {
            return Task.FromResult(
                AuthenticateResult
                    .Fail(StatusCodes.Status401Unauthorized
                        .ToString(CultureInfo.InvariantCulture)));
        }

        var claimsPrincipal = _tokenService.ConvertJwtStringToClaimsPrincipal(token, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
    }
}