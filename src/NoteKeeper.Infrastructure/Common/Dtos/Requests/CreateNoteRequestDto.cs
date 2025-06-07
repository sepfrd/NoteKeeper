using NoteKeeper.Domain.Entities;

namespace NoteKeeper.Infrastructure.Common.Dtos.Requests;

public record CreateNoteRequestDto
{
    public required string Title { get; set; }

    public required string Content { get; set; }

    public Note ToNoteDomainEntity() => new()
    {
        Title = Title,
        Content = Content
    };
}