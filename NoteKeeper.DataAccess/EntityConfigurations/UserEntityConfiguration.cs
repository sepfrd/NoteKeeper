using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.DataAccess.EntityConfigurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .HasIndex(user => user.Username)
            .IsUnique();

        builder
            .Property(user => user.Username)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(32);

        builder
            .Property(user => user.FirstName)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(100);

        builder
            .Property(user => user.LastName)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(100);

        builder
            .Property(user => user.PasswordHash)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(500);

        builder
            .Property(user => user.Email)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(320);

        builder.Ignore(user => user.FullName);
    }
}