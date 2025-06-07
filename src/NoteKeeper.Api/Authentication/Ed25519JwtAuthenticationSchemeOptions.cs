using Microsoft.AspNetCore.Authentication;

namespace NoteKeeper.Api.Authentication;

public class Ed25519JwtAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "Ed25519JwtAuthentication";
}