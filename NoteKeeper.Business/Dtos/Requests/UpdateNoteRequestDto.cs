namespace NoteKeeper.Business.Dtos.Requests;

public record UpdateNoteRequestDto
{
    public UpdateNoteRequestDto(string newTitle, string newContent)
    {
        NewTitle = newTitle;
        NewContent = newContent;
    }

    public required string NewTitle { get; init; }

    public required string NewContent { get; init; }
}