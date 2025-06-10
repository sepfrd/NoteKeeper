using Microsoft.EntityFrameworkCore;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Application.Interfaces.Repositories;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google.Data;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Notion.Data;
using NoteKeeper.Infrastructure.Interfaces;
using NoteKeeper.Infrastructure.Persistence.EntityConfigurations;
using NoteKeeper.Infrastructure.Persistence.Repositories;

namespace NoteKeeper.Infrastructure.Persistence;

public class UnitOfWork : DbContext, IUnitOfWork
{
    private readonly IDbConnectionPool _dbConnectionPool;

    private UserRepository? _userRepository;
    private NoteRepository? _noteRepository;
    private IExternalProviderAccountRepository? _externalProviderAccountRepository;
    private GoogleTokenRepository? _googleTokenRepository;
    private NotionTokenRepository? _notionTokenRepository;

    public UnitOfWork(DbContextOptions dbContextOptions, IDbConnectionPool dbConnectionPool) : base(dbContextOptions)
    {
        _dbConnectionPool = dbConnectionPool;
    }

    public DbSet<Note> Notes { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<ExternalProviderAccount> ExternalProviderAccounts { get; set; }

    public DbSet<GoogleToken> GoogleTokens { get; set; }

    public DbSet<NotionToken> NotionTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new DomainEntityConfiguration());
        modelBuilder.ApplyConfiguration(new GoogleTokenEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NoteEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NotionTokenEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
    }

    public IUserRepository UserRepository => _userRepository ??= new UserRepository(Users, _dbConnectionPool);

    public INoteRepository NoteRepository => _noteRepository ??= new NoteRepository(Notes, _dbConnectionPool);

    public IExternalProviderAccountRepository ExternalProviderAccountRepository => _externalProviderAccountRepository ??= new ExternalProviderAccountRepository(ExternalProviderAccounts, _dbConnectionPool);

    public INotionTokenRepository NotionTokenRepository => _notionTokenRepository ??= new NotionTokenRepository(NotionTokens, _dbConnectionPool);

    public IGoogleTokenRepository GoogleTokenRepository => _googleTokenRepository ??= new GoogleTokenRepository(GoogleTokens, _dbConnectionPool);

    public int CommitChanges() => SaveChanges();

    public async Task<int> CommitChangesAsync(CancellationToken cancellationToken = default) =>
        await SaveChangesAsync(cancellationToken);
}