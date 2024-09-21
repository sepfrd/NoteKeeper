using NoteKeeper.Business.Interfaces;
using NoteKeeper.DataAccess.Entities;
using NoteKeeper.DataAccess.Interfaces;

namespace NoteKeeper.Business.Services;

public class NoteService : INoteService
{
    private readonly IRepositoryBase<Note> _noteRepository;

    public NoteService(IRepositoryBase<Note> noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<Note> CreateAsync(Note note, CancellationToken cancellationToken = default) =>
        await _noteRepository.CreateAsync(note, cancellationToken);

    public async Task<List<Note>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        await _noteRepository.GetAllAsync(
            pageNumber,
            pageSize,
            null,
            null,
            cancellationToken);

    public async Task<Note?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        await _noteRepository.GetByIdAsync(id, null, cancellationToken);

    public async Task<Note?> UpdateAsync(long id, string newTitle, string newContent, CancellationToken cancellationToken = default)
    {
        var note = await _noteRepository.GetByIdAsync(id, null, cancellationToken);

        if (note is null)
        {
            return null;
        }

        note.Title = newTitle;
        note.Content = newContent;
        note.MarkAsUpdated();

        await _noteRepository.SaveChangesAsync(cancellationToken);

        return note;
    }

    public async Task<Note?> DeleteByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var note = await _noteRepository.GetByIdAsync(id, null, cancellationToken);

        if (note is null)
        {
            return null;
        }

        return _noteRepository.Delete(note);
    }
}