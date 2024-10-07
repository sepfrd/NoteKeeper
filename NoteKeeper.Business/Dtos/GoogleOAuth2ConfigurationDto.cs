namespace NoteKeeper.Business.Dtos;

public record GoogleOAuth2ConfigurationDto(
    string RedirectUri,
    string ClientId,
    string ProjectId,
    string AuthUri,
    string TokenUri,
    string RevokeUri,
    string AuthProviderX509CertUrl,
    string ClientSecret);