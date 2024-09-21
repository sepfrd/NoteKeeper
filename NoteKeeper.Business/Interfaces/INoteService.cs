using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.Business.Interfaces;

public interface INoteService
{
    Task<Note> CreateAsync(Note note, CancellationToken cancellationToken = default);

    Task<List<Note>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<Note?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Note?> UpdateAsync(long id, string newTitle, string newContent, CancellationToken cancellationToken = default);

    Task<Note?> DeleteByIdAsync(long id, CancellationToken cancellationToken = default);
}