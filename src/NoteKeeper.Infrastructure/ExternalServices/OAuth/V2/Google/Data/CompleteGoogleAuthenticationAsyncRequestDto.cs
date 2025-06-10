namespace NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google.Data;

public record CompleteGoogleAuthenticationAsyncRequestDto(
    string State,
    string Code,
    string Scope,
    string AuthUser,
    string Prompt);