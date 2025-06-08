namespace NoteKeeper.Infrastructure.Common.Dtos.Requests;

public record CreateNoteRequestDto
{
    public required string Title { get; set; }

    public required string Content { get; set; }
}