namespace NoteKeeper.Infrastructure.Common.Dtos.Google;

public record CompleteGoogleAuthenticationAsyncRequestDto(
    string State,
    string Code,
    string Scope,
    string AuthUser,
    string Prompt);