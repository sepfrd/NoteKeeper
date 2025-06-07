using BCrypt.Net;
using Org.BouncyCastle.Crypto.Parameters;

namespace NoteKeeper.Shared.Utilities;

public static class CryptographyHelper
{
    public static string HashPassword(string password) =>
        BCrypt.Net.BCrypt.EnhancedHashPassword(password, HashType.SHA512, 12);

    public static bool ValidatePassword(string password, string passwordHash) =>
        BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash, HashType.SHA512);

    public static Ed25519PrivateKeyParameters LoadPrivateKeyFromString(string privateKeyString)
    {
        var privateKeyBytes = Convert.FromBase64String(privateKeyString);

        return new Ed25519PrivateKeyParameters(privateKeyBytes, 0);
    }

    public static Ed25519PublicKeyParameters LoadPublicKeyFromString(string publicKeyString)
    {
        var publicKeyBytes = Convert.FromBase64String(publicKeyString);

        return new Ed25519PublicKeyParameters(publicKeyBytes, 0);
    }
}