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

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("Buildings");
        builder.HasKey(b => b.BuildingId);
        
        builder.Property(b => b.Name).HasMaxLength(100);
        builder.HasIndex(b => b.Name).IsUnique();

        builder.HasQueryFilter(b => !b.IsDeleted);

        // One-to-Many: Building -> Units
        builder.HasMany(b => b.Units)
            .WithOne(u => u.Building)
            .HasForeignKey(u => u.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.ProjectId);
    }
}

