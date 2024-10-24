namespace NoteKeeper.Business.Dtos.Notion;

public record NotionOAuth2ConfigurationDto(
    string RedirectUri,
    string ClientId,
    string AuthUri,
    string TokenUri,
    string ClientSecret);