namespace NoteKeeper.Business.Dtos.Google;

public record GoogleExchangeCodeForTokenRequestDto(
    string State,
    string Code,
    string Scope,
    string AuthUser,
    string Prompt);