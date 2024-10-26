using System.Text.Json.Serialization;
using Microsoft.IdentityModel.JsonWebTokens;
using NoteKeeper.Business.Constants;
using NoteKeeper.DataAccess.Entities;
using NoteKeeper.DataAccess.Enums;

namespace NoteKeeper.Business.Dtos.Google;

public record GoogleIdTokenPayloadDto
{
    [JsonPropertyName(JwtRegisteredClaimNames.Iss)]
    public string? Issuer { get; set; }

    [JsonPropertyName(JwtRegisteredClaimNames.Azp)]
    public string? AuthorizedParty { get; set; }

    [JsonPropertyName(JwtRegisteredClaimNames.Aud)]
    public required string Audience { get; set; }

    [JsonPropertyName(JwtRegisteredClaimNames.Sub)]
    public required string Subject { get; set; }

    [JsonPropertyName(JwtRegisteredClaimNames.Email)]
    public required string Email { get; set; }

    [JsonPropertyName(CustomOAuthConstants.EmailVerifiedJsonPropertyName)]
    public bool? EmailVerified { get; set; }

    [JsonPropertyName(JwtRegisteredClaimNames.AtHash)]
    public string? AccessTokenHashValue { get; set; }

    [JsonPropertyName(JwtRegisteredClaimNames.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(CustomOAuthConstants.PictureJsonPropertyName)]
    public string? Picture { get; set; }

    [JsonPropertyName(JwtRegisteredClaimNames.GivenName)]
    public string? GivenName { get; set; }

    [JsonPropertyName(JwtRegisteredClaimNames.FamilyName)]
    public string? FamilyName { get; set; }

    [JsonPropertyName(JwtRegisteredClaimNames.Iat)]
    public int? IssuedAt { get; set; }

    [JsonPropertyName(JwtRegisteredClaimNames.Exp)]
    public int? ExpiresAt { get; set; }

    public User ToUserDomainEntity() => new()
    {
        Username = Subject,
        Email = Email,
        FirstName = GivenName,
        LastName = FamilyName,
        RegistrationType = RegistrationType.Google
    };
}