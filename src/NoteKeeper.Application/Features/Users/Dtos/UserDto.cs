namespace NoteKeeper.Application.Features.Users.Dtos;

public record UserDto
{
    public UserDto(Guid uuid, string username, string email, string? fullName)
    {
        Uuid = uuid;
        Username = username;
        Email = email;
        FullName = fullName;
    }

    public Guid Uuid { get; init; }

    public string Username { get; init; }

    public string Email { get; init; }

    public string? FullName { get; init; }
}