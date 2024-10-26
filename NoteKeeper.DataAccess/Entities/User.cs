using NoteKeeper.DataAccess.Enums;

namespace NoteKeeper.DataAccess.Entities;

public class User : DomainEntity
{
    public required string Username { get; set; }

    public required string Email { get; set; }

    public string? PasswordHash { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public RegistrationType RegistrationType { get; set; } = RegistrationType.Direct;

    public string FullName => FirstName + ' ' + LastName;

    public GoogleToken? GoogleToken { get; set; }

    public NotionToken? NotionToken { get; set; }

    public ICollection<Note> Notes { get; set; } = [];
}