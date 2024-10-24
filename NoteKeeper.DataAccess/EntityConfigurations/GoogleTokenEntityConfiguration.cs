using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoteKeeper.DataAccess.Entities;

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
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(5000);

        builder
            .Property(googleToken => googleToken.RefreshToken)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(1000);

        builder
            .Property(googleToken => googleToken.Scope)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(1000);

        builder
            .Property(googleToken => googleToken.TokenType)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(100);

        builder
            .Property(googleToken => googleToken.IdToken)
            .HasColumnType(SqlDbType.VarChar.ToString())
            .HasMaxLength(5000);

        builder.Ignore(googleToken => googleToken.IsExpired);
    }
}