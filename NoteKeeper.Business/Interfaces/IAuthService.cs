using System.Security.Claims;
using NoteKeeper.Business.Dtos;
using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.Business.Interfaces;

public interface IAuthService
{
    Task<ResponseDto<string?>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

    bool IsEd25519JwtValid(string token);

    ClaimsPrincipal ConvertJwtStringToClaimsPrincipal(string jwtString, string authenticationType);

    Task<User> GetSignedInUserAsync(CancellationToken cancellationToken = default);
}