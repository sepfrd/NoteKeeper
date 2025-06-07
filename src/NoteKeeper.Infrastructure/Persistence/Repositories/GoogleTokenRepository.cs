using Microsoft.EntityFrameworkCore;
using NoteKeeper.Application.Interfaces.Repositories;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Infrastructure.Interfaces;

namespace NoteKeeper.Infrastructure.Persistence.Repositories;

public class GoogleTokenRepository : RepositoryBase<GoogleToken, long>, IGoogleTokenRepository
{
    public GoogleTokenRepository(DbSet<GoogleToken> dbSet, IDbConnectionPool dbConnectionPool)
        : base(dbSet, dbConnectionPool)
    {
    }
}