namespace NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Notion.Data;

public record NotionExchangeCodeForTokenRequestDto(
    string State,
    string Code,
    string Error);