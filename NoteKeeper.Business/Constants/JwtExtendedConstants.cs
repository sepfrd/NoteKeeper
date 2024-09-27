namespace NoteKeeper.Business.Constants;

public static class JwtExtendedConstants
{
    public const string JwtAlgorithmEdDsa = "EdDSA";
    public const string JwtCurveEd25519 = "Ed25519";
    public const string JwtKeyTypeOkp = "OKP";
    public const string JwtHeaderCurveKey = "crv";
    public const string JwtHeaderKeyTypeKey = "kty";
    public const string JwtUsernameClaimType = "username";
    public const string JwtUuidClaimType = "uuid";
}