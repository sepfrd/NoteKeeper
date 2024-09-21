namespace NoteKeeper.Business.Dtos;

public record JwtConfigurationDto(string PrivateKey, string PublicKey, string Issuer, string Audience, int TokenLifetimeInSeconds);