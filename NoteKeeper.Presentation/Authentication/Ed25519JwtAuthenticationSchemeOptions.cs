using Microsoft.AspNetCore.Authentication;

namespace NoteKeeper.Presentation.Authentication;

public class Ed25519JwtAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "Ed25519JwtAuthentication";
}