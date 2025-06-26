namespace NoteKeeper.Infrastructure.Common.Dtos.Requests;

public record LoginRequestDto(string UsernameOrEmail, string Password);