using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoteKeeper.Domain.Entities;
using NpgsqlTypes;

namespace NoteKeeper.Infrastructure.Persistence.EntityConfigurations;

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
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(5000);

        builder
            .Property(notionToken => notionToken.TokenType)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(100);

        builder
            .Property(notionToken => notionToken.BotId)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(100);

        builder
            .Property(notionToken => notionToken.WorkspaceName)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(150);

        builder
            .Property(notionToken => notionToken.WorkspaceIconUrl)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(250);

        builder
            .Property(notionToken => notionToken.WorkspaceId)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(100);

        builder
            .Property(notionToken => notionToken.NotionId)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(100);

        builder
            .Property(notionToken => notionToken.Name)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(100);

        builder
            .Property(notionToken => notionToken.AvatarUrl)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(250);

        builder
            .Property(notionToken => notionToken.NotionEmail)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(320);

        builder.Ignore(notionToken => notionToken.IsExpired);
    }
}