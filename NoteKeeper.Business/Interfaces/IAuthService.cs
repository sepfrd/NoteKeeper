using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Query;
using NoteKeeper.Business.Dtos;
using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.Business.Interfaces;

public interface IAuthService
{
    Task<ResponseDto<string?>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

    bool IsEd25519JwtValid(string token);

    ClaimsPrincipal ConvertJwtStringToClaimsPrincipal(string jwtString, string authenticationType);

    Task<User?> GetSignedInUserAsync(Func<IQueryable<User>, IIncludableQueryable<User, object?>>? include = null, CancellationToken cancellationToken = default);

    Task<ResponseDto<string?>> UseGoogleOAuth2Async(CancellationToken cancellationToken = default);

    Task<ResponseDto<string?>> GoogleExchangeCodeForTokenAsync(GoogleExchangeCodeForTokenRequestDto exchangeCodeForTokenRequestDto, CancellationToken cancellationToken = default);
}