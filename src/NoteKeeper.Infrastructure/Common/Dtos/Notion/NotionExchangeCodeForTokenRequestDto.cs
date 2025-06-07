namespace NoteKeeper.Infrastructure.Common.Dtos.Notion;

public record NotionExchangeCodeForTokenRequestDto(
    string State,
    string Code,
    string Error);