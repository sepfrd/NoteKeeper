namespace NoteKeeper.Application.Features.Notes.Dtos;

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
}