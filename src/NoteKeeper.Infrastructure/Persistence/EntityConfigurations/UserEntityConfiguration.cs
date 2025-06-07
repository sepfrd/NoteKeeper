using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoteKeeper.Domain.Entities;
using NpgsqlTypes;

namespace NoteKeeper.Infrastructure.Persistence.EntityConfigurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .HasIndex(user => user.Username)
            .IsUnique();

        builder
            .Property(user => user.Username)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(32);

        builder
            .Property(user => user.FirstName)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(100);

        builder
            .Property(user => user.LastName)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(100);

        builder
            .Property(user => user.PasswordHash)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(500);

        builder
            .Property(user => user.Email)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(320);

        builder.Ignore(user => user.FullName);
    }
}