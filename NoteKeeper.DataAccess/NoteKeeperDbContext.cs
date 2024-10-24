using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using NoteKeeper.DataAccess.Entities;
using NoteKeeper.DataAccess.EntityConfigurations;

namespace NoteKeeper.DataAccess;

public class NoteKeeperDbContext : DbContext
{
    public NoteKeeperDbContext(DbContextOptions<NoteKeeperDbContext> options) : base(options)
    {
    }

    public DbSet<Note> Notes { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<GoogleToken> GoogleTokens { get; set; }

    public DbSet<NotionToken> NotionTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new GoogleTokenEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NoteEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NotionTokenEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());

        modelBuilder
            .Entity<User>()
            .HasData([
                new User
                {
                    Id = 1L,
                    Username = "sepehr_frd",
                    Email = "sepfrd@outlook.com",
                    PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword("Sfr1376.", HashType.SHA512, 12)
                }
            ]);
    }
}