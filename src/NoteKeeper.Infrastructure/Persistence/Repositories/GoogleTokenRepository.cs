using Microsoft.EntityFrameworkCore;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google.Data;
using NoteKeeper.Infrastructure.Interfaces;

namespace NoteKeeper.Infrastructure.Persistence.Repositories;

public class GoogleTokenRepository : RepositoryBase<GoogleToken, long>, IGoogleTokenRepository
{
    public GoogleTokenRepository(DbSet<GoogleToken> dbSet, IDbConnectionPool dbConnectionPool)
        : base(dbSet, dbConnectionPool)
    {
    }
}