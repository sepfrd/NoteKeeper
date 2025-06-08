namespace NoteKeeper.Infrastructure.ExternalServices.Google.Data;

public record CompleteGoogleAuthenticationAsyncRequestDto(
    string State,
    string Code,
    string Scope,
    string AuthUser,
    string Prompt);