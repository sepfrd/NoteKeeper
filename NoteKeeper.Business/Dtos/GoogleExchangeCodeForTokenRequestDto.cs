namespace NoteKeeper.Business.Dtos;

public record GoogleExchangeCodeForTokenRequestDto(
    string State,
    string Code,
    string Scope,
    string AuthUser,
    string Prompt);