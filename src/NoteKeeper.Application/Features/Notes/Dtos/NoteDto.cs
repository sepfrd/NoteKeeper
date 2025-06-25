namespace NoteKeeper.Application.Features.Notes.Dtos;

public record NoteDto
{
    public Guid Uuid { get; init; }

    public string? Title { get; init; }

    public string? Content { get; init; }

    public Guid UserUuid { get; set; }
}