namespace NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Notion.Models;

public record NotionExchangeCodeForTokenRequestDto(
    string State,
    string Code,
    string Error);