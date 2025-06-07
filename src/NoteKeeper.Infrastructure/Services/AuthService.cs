using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Options;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Application.Interfaces.Repositories;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Domain.Enums;
using NoteKeeper.Infrastructure.Common.Constants;
using NoteKeeper.Infrastructure.Common.Dtos;
using NoteKeeper.Infrastructure.Common.Dtos.Configurations;
using NoteKeeper.Infrastructure.Common.Dtos.Responses;
using NoteKeeper.Shared.Utilities;

namespace NoteKeeper.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IRedisService _redisService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppOptions _appOptions;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IRedisService redisService,
        IHttpContextAccessor httpContextAccessor,
        IOptions<AppOptions> appOptions)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _redisService = redisService;
        _httpContextAccessor = httpContextAccessor;
        _appOptions = appOptions.Value;
    }

    public async Task<ResponseDto<AuthResponseDto?>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        if (IsSignedIn())
        {
            return new ResponseDto<AuthResponseDto?>
            {
                IsSuccess = false,
                Message = MessageConstants.AlreadySignedInMessage,
                HttpStatusCode = HttpStatusCode.BadRequest
            };
        }

        var user = await GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail, cancellationToken);

        if (user is null ||
            user.RegistrationType != RegistrationType.Direct ||
            !RegexValidator.PasswordRegex().IsMatch(loginDto.Password))
        {
            return new ResponseDto<AuthResponseDto?>
            {
                IsSuccess = false,
                Message = MessageConstants.InvalidCredentialsMessage,
                HttpStatusCode = HttpStatusCode.BadRequest
            };
        }

        var isPasswordValid = CryptographyHelper.ValidatePassword(loginDto.Password, user.PasswordHash!);

        if (!isPasswordValid)
        {
            return new ResponseDto<AuthResponseDto?>
            {
                IsSuccess = false,
                Message = MessageConstants.InvalidCredentialsMessage,
                HttpStatusCode = HttpStatusCode.BadRequest
            };
        }

        var jwt = _tokenService.GenerateEd25519Jwt(user);

        var refreshToken = await _tokenService.GenerateNewRefreshTokenAsync(user.Id.ToString());

        return new ResponseDto<AuthResponseDto?>
        {
            Data = new AuthResponseDto(
                jwt,
                refreshToken,
                DateTimeOffset.UtcNow.AddMinutes(_appOptions.JwtOptions.RefreshTokenLifeTimeInMinutes)),
            IsSuccess = true,
            Message = MessageConstants.SuccessfulLoginMessage,
            HttpStatusCode = HttpStatusCode.OK
        };
    }

    public async Task<ResponseDto<AuthResponseDto?>> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var redisAuthDatabase = _appOptions.RedisOptions.Databases.Auth;
        var existingRefreshTokenKey = string.Format(RedisConstants.RefreshTokenStringKeyTemplate, refreshToken);

        var redisValue = await _redisService.GetDeleteStringAsync(existingRefreshTokenKey, redisAuthDatabase);

        if (redisValue.IsNullOrEmpty)
        {
            return new ResponseDto<AuthResponseDto?>
            {
                IsSuccess = false,
                HttpStatusCode = HttpStatusCode.Unauthorized
            };
        }

        // generate a new JWT, refresh token, invalidate the previous token and save to Redis.

        var userId = (long)redisValue;
        var user = await _userRepository.GetByIdAsync(userId, null, cancellationToken);

        var jwt = _tokenService.GenerateEd25519Jwt(user!);

        var newRefreshToken = await _tokenService.GenerateNewRefreshTokenAsync(userId.ToString());

        return new ResponseDto<AuthResponseDto?>
        {
            Data = new AuthResponseDto(
                jwt,
                newRefreshToken,
                DateTimeOffset.UtcNow.AddMinutes(_appOptions.JwtOptions.RefreshTokenLifeTimeInMinutes)),
            IsSuccess = true,
            Message = MessageConstants.SuccessfulTokenRefreshMessage,
            HttpStatusCode = HttpStatusCode.OK
        };
    }

    public async Task<User?> GetSignedInUserAsync(
        Func<IQueryable<User>, IIncludableQueryable<User, object?>>? include = null,
        CancellationToken cancellationToken = default)
    {
        var userUuid = _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtExtendedConstants.JwtUuidClaimType);

        if (userUuid is null)
        {
            return null;
        }

        var user = await _userRepository.GetByUuidAsync(Guid.Parse(userUuid), include, cancellationToken);

        return user;
    }

    private bool IsSignedIn() =>
        _httpContextAccessor.HttpContext!.User.Identity is not null && _httpContextAccessor.HttpContext!.User.Identity.IsAuthenticated;

    private async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
    {
        User? user;

        if (RegexValidator.UsernameRegex().IsMatch(usernameOrEmail))
        {
            user = await _userRepository.GetByUsernameAsync(usernameOrEmail, null, cancellationToken);
        }
        else
        {
            user = await _userRepository.GetByEmailAsync(usernameOrEmail, null, cancellationToken);
        }

        return user;
    }
}