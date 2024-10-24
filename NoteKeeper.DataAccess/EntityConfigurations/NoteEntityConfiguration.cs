using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.DataAccess.EntityConfigurations;

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
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(200);

        builder
            .Property(note => note.Content)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(2000);
    }
}