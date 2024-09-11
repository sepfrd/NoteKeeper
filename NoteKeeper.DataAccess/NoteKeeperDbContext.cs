using System.Data;
using Microsoft.EntityFrameworkCore;
using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.DataAccess;

public class NoteKeeperDbContext : DbContext
{
    public NoteKeeperDbContext(DbContextOptions<NoteKeeperDbContext> options) : base(options)
    {
    }

    public DbSet<Note> Notes { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<DomainEntity>()
            .HasKey(entity => entity.Id);

        modelBuilder
            .Entity<Note>()
            .HasOne<User>(note => note.User)
            .WithMany(user => user.Notes)
            .HasForeignKey(note => note.UserId);

        modelBuilder
            .Entity<Note>()
            .Property(note => note.Title)
            .HasColumnType(SqlDbType.NVarChar.ToString())
            .HasMaxLength(200);

        modelBuilder
            .Entity<Note>()
            .Property(note => note.Content)
            .HasColumnType(SqlDbType.NVarChar.ToString())
            .HasMaxLength(2000);

        modelBuilder
            .Entity<User>()
            .HasIndex(user => user.Username)
            .IsUnique();

        modelBuilder
            .Entity<User>()
            .Property(user => user.Username)
            .HasColumnType(SqlDbType.NVarChar.ToString())
            .HasMaxLength(32);

        modelBuilder
            .Entity<User>()
            .Property(user => user.FirstName)
            .HasColumnType(SqlDbType.NVarChar.ToString())
            .HasMaxLength(100);

        modelBuilder
            .Entity<User>()
            .Property(user => user.LastName)
            .HasColumnType(SqlDbType.NVarChar.ToString())
            .HasMaxLength(100);

        modelBuilder
            .Entity<User>()
            .Property(user => user.PasswordHash)
            .HasColumnType(SqlDbType.NVarChar.ToString())
            .HasMaxLength(500);

        modelBuilder
            .Entity<User>()
            .Property(user => user.Email)
            .HasColumnType(SqlDbType.NVarChar.ToString())
            .HasMaxLength(320);

        modelBuilder
            .Entity<User>()
            .Ignore(user => user.FullName);
    }
}