namespace NoteKeeper.DataAccess.Entities;

public class User : DomainEntity
{
    public required string Username { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string FullName => FirstName + ' ' + LastName;

    public ICollection<Note> Notes { get; set; } = [];
}