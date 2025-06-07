using System.Security.Claims;
using NoteKeeper.Domain.Entities;

namespace NoteKeeper.Application.Interfaces;

public interface ITokenService
{
    bool IsEd25519JwtValid(string token);

    ClaimsPrincipal ConvertJwtStringToClaimsPrincipal(string jwtString, string authenticationType);

    string GenerateEd25519Jwt(User user);

    Task<string> GenerateNewRefreshTokenAsync(string userIdString);
}