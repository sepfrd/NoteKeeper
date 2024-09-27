using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.Business.Dtos.DomainEntities;

public record NoteDto
{
    public NoteDto(Guid uuid, string title, string content)
    {
        Uuid = uuid;
        Title = title;
        Content = content;
    }

    public Guid Uuid { get; init; }

    public string Title { get; init; }

    public string Content { get; init; }

    public Guid UserUuid { get; set; }

    public static NoteDto FromNoteDomainEntity(Note note) => new(note.Uuid, note.Title, note.Content);
}