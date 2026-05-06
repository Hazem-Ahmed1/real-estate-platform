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

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Units");
        builder.HasKey(u => u.UnitId);

        builder.Property(u => u.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(u => u.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(u => u.Street).HasMaxLength(255);
        builder.Property(u => u.Price).HasPrecision(18, 2);

        // One-to-Many: Unit -> Media
        builder.HasMany(u => u.Media)
            .WithOne(m => m.Unit)
            .HasForeignKey(m => m.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-Many: Unit -> NearbyFacilities
        builder.HasMany(u => u.NearbyFacilities)
            .WithOne(nf => nf.Unit)
            .HasForeignKey(nf => nf.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(u => u.BuildingId);
        builder.HasIndex(u => u.Price);
    }
}

