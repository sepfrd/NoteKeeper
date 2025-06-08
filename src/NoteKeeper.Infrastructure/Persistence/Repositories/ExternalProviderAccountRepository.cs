using Microsoft.EntityFrameworkCore;
using NoteKeeper.Application.Interfaces.Repositories;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Infrastructure.Interfaces;

namespace NoteKeeper.Infrastructure.Persistence.Repositories;

public class ExternalProviderAccountRepository : RepositoryBase<ExternalProviderAccount, long>, IExternalProviderAccountRepository
{
    public ExternalProviderAccountRepository(DbSet<ExternalProviderAccount> dbSet, IDbConnectionPool dbConnectionPool)
        : base(dbSet, dbConnectionPool)
    {
    }
}