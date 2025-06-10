using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google.Data;
using NpgsqlTypes;

namespace NoteKeeper.Infrastructure.Persistence.EntityConfigurations;

public class GoogleTokenEntityConfiguration : IEntityTypeConfiguration<GoogleToken>
{
    public void Configure(EntityTypeBuilder<GoogleToken> builder)
    {
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