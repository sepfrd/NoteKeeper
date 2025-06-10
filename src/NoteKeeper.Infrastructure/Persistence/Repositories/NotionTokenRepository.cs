using Microsoft.EntityFrameworkCore;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Notion.Data;
using NoteKeeper.Infrastructure.Interfaces;

namespace NoteKeeper.Infrastructure.Persistence.Repositories;

public class NotionTokenRepository : RepositoryBase<NotionToken, long>, INotionTokenRepository
{
    public NotionTokenRepository(DbSet<NotionToken> dbSet, IDbConnectionPool dbConnectionPool)
        : base(dbSet, dbConnectionPool)
    {
    }
}