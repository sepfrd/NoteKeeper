namespace NoteKeeper.Infrastructure.Common.Dtos.Responses;

public record AuthResponseDto(string Jwt, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt);