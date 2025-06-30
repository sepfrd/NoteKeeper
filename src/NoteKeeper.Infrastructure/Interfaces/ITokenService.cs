using NoteKeeper.Domain.Entities;

namespace NoteKeeper.Infrastructure.Interfaces;

public interface ITokenService
{
    bool ValidateEd25519Jwt(string token, out Dictionary<string, object>? claims);

    string GenerateEd25519Jwt(User user);

    Task<string> GenerateNewRefreshTokenAsync(string userIdString);
}