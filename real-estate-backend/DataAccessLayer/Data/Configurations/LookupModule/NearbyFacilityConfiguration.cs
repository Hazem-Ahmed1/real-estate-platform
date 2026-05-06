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

public class NearbyFacilityConfiguration : IEntityTypeConfiguration<NearbyFacility>
{
    public void Configure(EntityTypeBuilder<NearbyFacility> builder)
    {
        builder.ToTable("Nearby_Facilities");
        builder.HasKey(nf => nf.FacilityId);

        builder.Property(nf => nf.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(nf => nf.Name).IsRequired().HasMaxLength(255);
        builder.Property(nf => nf.Distance).HasMaxLength(50);
    }
}

