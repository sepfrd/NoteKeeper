namespace NoteKeeper.Business.Dtos.Configurations;

public record JwtOptions(
    string PrivateKey,
    string PublicKey,
    string Issuer,
    string Audience,
    double TokenLifetimeInSeconds,
    double RefreshTokenLifeTimeInMinutes);