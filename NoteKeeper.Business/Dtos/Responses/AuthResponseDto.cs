namespace NoteKeeper.Business.Dtos.Responses;

public record AuthResponseDto(string Jwt, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt);