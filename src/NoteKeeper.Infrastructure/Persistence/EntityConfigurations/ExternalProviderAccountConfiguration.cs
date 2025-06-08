using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoteKeeper.Domain.Entities;
using NpgsqlTypes;

namespace NoteKeeper.Infrastructure.Persistence.EntityConfigurations;

public class ExternalProviderAccountConfiguration : IEntityTypeConfiguration<ExternalProviderAccount>
{
    public void Configure(EntityTypeBuilder<ExternalProviderAccount> builder)
    {
        builder
            .HasOne<User>(account => account.User)
            .WithMany(user => user.ExternalProviderAccounts)
            .HasForeignKey(account => account.UserId);

        builder
            .Property(account => account.ProviderName)
            .HasColumnType(nameof(NpgsqlDbType.Varchar))
            .HasMaxLength(200);
    }
}