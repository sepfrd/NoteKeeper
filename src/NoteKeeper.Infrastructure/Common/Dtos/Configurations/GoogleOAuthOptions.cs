namespace NoteKeeper.Infrastructure.Common.Dtos.Configurations;

public record GoogleOAuthOptions(
    string RedirectUri,
    string ClientId,
    string ProjectId,
    string AuthUri,
    string TokenUri,
    string RevokeUri,
    string AuthProviderX509CertUrl,
    string ClientSecret);