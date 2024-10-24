using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.DataAccess.EntityConfigurations;

public class NotionTokenEntityConfiguration : IEntityTypeConfiguration<NotionToken>
{
    public void Configure(EntityTypeBuilder<NotionToken> builder)
    {
        builder
            .HasOne<User>(notionToken => notionToken.User)
            .WithOne(user => user.NotionToken)
            .HasForeignKey<NotionToken>(notionToken => notionToken.UserId);

        builder
            .Property(notionToken => notionToken.AccessToken)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(5000);

        builder
            .Property(notionToken => notionToken.TokenType)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(100);

        builder
            .Property(notionToken => notionToken.BotId)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(100);

        builder
            .Property(notionToken => notionToken.WorkspaceName)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(150);

        builder
            .Property(notionToken => notionToken.WorkspaceIconUrl)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(250);

        builder
            .Property(notionToken => notionToken.WorkspaceId)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(100);

        builder
            .Property(notionToken => notionToken.NotionId)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(100);

        builder
            .Property(notionToken => notionToken.Name)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(100);

        builder
            .Property(notionToken => notionToken.AvatarUrl)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(250);

        builder
            .Property(notionToken => notionToken.NotionEmail)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(320);

        builder.Ignore(notionToken => notionToken.IsExpired);
    }
}