using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Domain.Common;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Domain.Enums;
using NoteKeeper.Infrastructure.Common.Constants;
using NoteKeeper.Infrastructure.Common.Dtos;
using NoteKeeper.Infrastructure.Common.Dtos.Configurations;
using NoteKeeper.Infrastructure.Common.Dtos.Responses;
using NoteKeeper.Infrastructure.Interfaces;
using NoteKeeper.Shared.Resources;
using NoteKeeper.Shared.Utilities;

namespace NoteKeeper.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IRedisService _redisService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppOptions _appOptions;

    public AuthService(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IRedisService redisService,
        IHttpContextAccessor httpContextAccessor,
        IOptions<AppOptions> appOptions)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _redisService = redisService;
        _httpContextAccessor = httpContextAccessor;
        _appOptions = appOptions.Value;
    }

    public async Task<DomainResult<AuthResponseDto?>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        if (IsSignedIn())
        {
            return DomainResult<AuthResponseDto?>.CreateFailure(ErrorMessages.AlreadySignedIn, StatusCodes.Status400BadRequest);
        }

        var user = await GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail, cancellationToken);

        if (user is null ||
            user.RegistrationType != RegistrationType.Direct ||
            !RegexValidator.PasswordRegex().IsMatch(loginDto.Password))
        {
            return DomainResult<AuthResponseDto?>.CreateFailure(ErrorMessages.InvalidCredentials, StatusCodes.Status400BadRequest);
        }

        var isPasswordValid = CryptographyHelper.ValidatePassword(loginDto.Password, user.PasswordHash!);

        if (!isPasswordValid)
        {
            return DomainResult<AuthResponseDto?>.CreateFailure(ErrorMessages.InvalidCredentials, StatusCodes.Status400BadRequest);
        }

        var jwt = _tokenService.GenerateEd25519Jwt(user);

        var refreshToken = await _tokenService.GenerateNewRefreshTokenAsync(user.Id.ToString());

        return DomainResult<AuthResponseDto?>.CreateSuccess(SuccessMessages.Login, StatusCodes.Status200OK, new AuthResponseDto(
            jwt,
            refreshToken,
            DateTimeOffset.UtcNow.AddMinutes(_appOptions.JwtOptions.RefreshTokenLifeTimeInMinutes)));
    }

    public async Task<DomainResult<AuthResponseDto?>> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var redisAuthDatabase = _appOptions.RedisOptions.Databases.Auth;
        var existingRefreshTokenKey = string.Format(RedisConstants.RefreshTokenStringKeyTemplate, refreshToken);

        var redisValue = await _redisService.GetDeleteStringAsync(existingRefreshTokenKey, redisAuthDatabase);

        if (redisValue.IsNullOrEmpty)
        {
            return DomainResult<AuthResponseDto?>.CreateFailure(ErrorMessages.Unauthorized, StatusCodes.Status401Unauthorized);
        }

        var userId = (long)redisValue;

        var user = await _unitOfWork.UserRepository.GetOneAsync(
            user => user.Id == userId,
            disableTracking: true,
            cancellationToken: cancellationToken);

        var jwt = _tokenService.GenerateEd25519Jwt(user!);

        var newRefreshToken = await _tokenService.GenerateNewRefreshTokenAsync(userId.ToString());

        return DomainResult<AuthResponseDto?>.CreateSuccess(SuccessMessages.TokenRefresh, StatusCodes.Status200OK, new AuthResponseDto(
            jwt,
            newRefreshToken,
            DateTimeOffset.UtcNow.AddMinutes(_appOptions.JwtOptions.RefreshTokenLifeTimeInMinutes)));
    }

    public string GetSignedInUserUuid() =>
        _httpContextAccessor
            .HttpContext?
            .User
            .Claims
            .First(claim => claim.Type == JwtExtendedConstants.JwtUuidClaimType)
            .Value!;

    public async Task<User?> GetSignedInUserAsync(
        IEnumerable<Expression<Func<User, object?>>>? includes = null,
        CancellationToken cancellationToken = default)
    {
        var userUuid = _httpContextAccessor
            .HttpContext?
            .User
            .Claims
            .FirstOrDefault(claim => claim.Type == JwtExtendedConstants.JwtUuidClaimType)?
            .Value;

        if (userUuid is null)
        {
            return null;
        }

        var user = await _unitOfWork.UserRepository.GetOneAsync(
            user => user.Uuid == Guid.Parse(userUuid),
            includes,
            disableTracking: true,
            cancellationToken: cancellationToken);

        return user;
    }

    private bool IsSignedIn() =>
        _httpContextAccessor.HttpContext!.User.Identity is not null && _httpContextAccessor.HttpContext!.User.Identity.IsAuthenticated;

    private async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
    {
        User? user;

        if (RegexValidator.UsernameRegex().IsMatch(usernameOrEmail))
        {
            user = await _unitOfWork.UserRepository.GetOneAsync(
                userEntity => userEntity.Username == usernameOrEmail,
                disableTracking: true,
                cancellationToken: cancellationToken);
        }
        else
        {
            user = await _unitOfWork.UserRepository.GetOneAsync(
                userEntity => userEntity.Email == usernameOrEmail,
                disableTracking: true,
                cancellationToken: cancellationToken);
        }

        return user;
    }
}