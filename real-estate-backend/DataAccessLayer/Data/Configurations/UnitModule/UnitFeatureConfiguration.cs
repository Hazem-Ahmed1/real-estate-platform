

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Data.Configurations;

public class UnitFeatureConfiguration : IEntityTypeConfiguration<UnitFeature>
{
    public void Configure(EntityTypeBuilder<UnitFeature> builder)
    {
        builder.ToTable("Unit_Features");
        builder.HasKey(uf => new { uf.UnitId, uf.FeatureId });

        builder.HasOne(uf => uf.Unit)
            .WithMany(u => u.UnitFeatures)
            .HasForeignKey(uf => uf.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uf => uf.Feature)
            .WithMany(f => f.UnitFeatures)
            .HasForeignKey(uf => uf.FeatureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

