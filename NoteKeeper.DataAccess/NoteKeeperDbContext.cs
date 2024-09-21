using System.Data;
using BCrypt.Net;
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
            .Entity<Note>()
            .HasOne<User>(note => note.User)
            .WithMany(user => user.Notes)
            .HasForeignKey(note => note.UserId);

        modelBuilder
            .Entity<Note>()
            .Property(note => note.Title)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(200);

        modelBuilder
            .Entity<Note>()
            .Property(note => note.Content)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(2000);

        modelBuilder
            .Entity<User>()
            .HasIndex(user => user.Username)
            .IsUnique();

        modelBuilder
            .Entity<User>()
            .Property(user => user.Username)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(32);

        modelBuilder
            .Entity<User>()
            .Property(user => user.FirstName)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(100);

        modelBuilder
            .Entity<User>()
            .Property(user => user.LastName)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(100);

        modelBuilder
            .Entity<User>()
            .Property(user => user.PasswordHash)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(500);

        modelBuilder
            .Entity<User>()
            .Property(user => user.Email)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(320);

        modelBuilder
            .Entity<User>()
            .Ignore(user => user.FullName);

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