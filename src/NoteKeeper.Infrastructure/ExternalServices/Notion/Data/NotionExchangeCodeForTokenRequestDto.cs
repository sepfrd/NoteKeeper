namespace NoteKeeper.Infrastructure.ExternalServices.Notion.Data;

public record NotionExchangeCodeForTokenRequestDto(
    string State,
    string Code,
    string Error);