namespace NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google.Models;

public record CompleteGoogleAuthenticationRequestDto(
    string State,
    string Code,
    string Scope,
    string AuthUser,
    string Prompt);