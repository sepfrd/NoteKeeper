using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoteKeeper.Domain.Entities;
using NpgsqlTypes;

namespace NoteKeeper.Infrastructure.Persistence.EntityConfigurations;

public class NoteEntityConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder
            .HasOne<User>(note => note.User)
            .WithMany(user => user.Notes)
            .HasForeignKey(note => note.UserId);

        builder
            .Property(note => note.Title)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(200);

        builder
            .Property(note => note.Content)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(2000);
    }
}