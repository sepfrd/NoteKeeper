using NoteKeeper.Domain.Entities;

namespace NoteKeeper.Application.Interfaces.Repositories;

public interface INoteRepository : IDomainEntityRepositoryBase<Note>;