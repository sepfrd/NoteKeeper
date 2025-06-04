namespace NoteKeeper.Business.Dtos.Configurations;

public record NotionOAuthOptions(
    string RedirectUri,
    string ClientId,
    string AuthUri,
    string TokenUri,
    string ClientSecret);