namespace NoteKeeper.Business.Dtos.Notion;

public record NotionExchangeCodeForTokenRequestDto(
    string State,
    string Code,
    string Error);