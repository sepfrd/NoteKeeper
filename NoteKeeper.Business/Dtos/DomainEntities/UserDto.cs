using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.Business.Dtos.DomainEntities;

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

    public static UserDto FromUserDomainEntity(User user) => new(user.Uuid, user.Username, user.Email, user.FullName);
}