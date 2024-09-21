using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using NoteKeeper.Business.Constants;

namespace NoteKeeper.Business.Dtos;

public class EdDSAJwtHeader
{
    public EdDSAJwtHeader(
        string algorithm = JwtExtendedConstants.JwtAlgorithmEdDsa,
        string type = JwtConstants.TokenType,
        string curve = JwtExtendedConstants.JwtCurveEd25519,
        string keyType = JwtExtendedConstants.JwtKeyTypeOkp,
        string? keyId = null)
    {
        Algorithm = algorithm;
        Type = type;
        Curve = curve;
        KeyType = keyType;
        KeyId = keyId;
    }

    [JsonPropertyName(JwtHeaderParameterNames.Alg)]
    public string Algorithm { get; set; }

    [JsonPropertyName(JwtHeaderParameterNames.Typ)]
    public string Type { get; set; }

    [JsonPropertyName(JwtExtendedConstants.JwtHeaderCurveKey)]

    public string Curve { get; set; }

    [JsonPropertyName(JwtExtendedConstants.JwtHeaderKeyTypeKey)]
    public string KeyType { get; set; }

    [JsonPropertyName(JwtHeaderParameterNames.Kid)]
    public string? KeyId { get; set; }
}