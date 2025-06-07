using Microsoft.EntityFrameworkCore;
using NoteKeeper.Application.Interfaces.Repositories;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Infrastructure.Interfaces;

namespace NoteKeeper.Infrastructure.Persistence.Repositories;

public class NotionTokenRepository : RepositoryBase<NotionToken, long>, INotionTokenRepository
{
    public NotionTokenRepository(DbSet<NotionToken> dbSet, IDbConnectionPool dbConnectionPool)
        : base(dbSet, dbConnectionPool)
    {
    }
}