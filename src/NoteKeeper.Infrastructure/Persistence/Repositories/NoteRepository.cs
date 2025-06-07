using Microsoft.EntityFrameworkCore;
using NoteKeeper.Application.Interfaces.Repositories;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Infrastructure.Interfaces;

namespace NoteKeeper.Infrastructure.Persistence.Repositories;

public class NoteRepository : RepositoryBase<Note, long>, INoteRepository
{
    public NoteRepository(DbSet<Note> dbSet, IDbConnectionPool dbConnectionPool)
        : base(dbSet, dbConnectionPool)
    {
    }
}