using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoteKeeper.DataAccess.Entities;
using NpgsqlTypes;

namespace NoteKeeper.DataAccess.EntityConfigurations;

public class GoogleTokenEntityConfiguration : IEntityTypeConfiguration<GoogleToken>
{
    public void Configure(EntityTypeBuilder<GoogleToken> builder)
    {
        builder
            .HasOne<User>(googleToken => googleToken.User)
            .WithOne(user => user.GoogleToken)
            .HasForeignKey<GoogleToken>(googleToken => googleToken.UserId);

        builder
            .Property(googleToken => googleToken.AccessToken)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(5000);

        builder
            .Property(googleToken => googleToken.RefreshToken)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(1000);

        builder
            .Property(googleToken => googleToken.Scope)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(1000);

        builder
            .Property(googleToken => googleToken.TokenType)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(100);

        builder
            .Property(googleToken => googleToken.IdToken)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(5000);

        builder.Ignore(googleToken => googleToken.IsExpired);
    }
}