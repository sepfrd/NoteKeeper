using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.DataAccess.EntityConfigurations;

public class DomainEntityConfiguration : IEntityTypeConfiguration<DomainEntity>
{
    public void Configure(EntityTypeBuilder<DomainEntity> builder)
    {
        builder.UseTpcMappingStrategy();

        builder.HasKey(entity => entity.Id);

        builder
            .HasIndex(entity => entity.Uuid)
            .IsUnique();
    }
}