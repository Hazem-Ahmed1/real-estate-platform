using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Data.Configurations;

public class UnitMediaConfiguration : IEntityTypeConfiguration<UnitMedia>
{
    public void Configure(EntityTypeBuilder<UnitMedia> builder)
    {
        builder.ToTable("Unit_Media");
        builder.HasKey(um => um.MediaId);

        builder.Property(um => um.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(um => um.MediaUrl).IsRequired();

        builder.HasIndex(um => um.UnitId);
        builder.HasIndex(um => new { um.UnitId, um.IsThumbnail });
    }
}

