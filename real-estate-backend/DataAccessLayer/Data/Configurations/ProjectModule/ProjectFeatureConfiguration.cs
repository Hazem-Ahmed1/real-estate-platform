

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Data.Configurations;

public class ProjectFeatureConfiguration : IEntityTypeConfiguration<ProjectFeature>
{
    public void Configure(EntityTypeBuilder<ProjectFeature> builder)
    {
        builder.ToTable("Project_Features");
        builder.HasKey(pf => new { pf.ProjectId, pf.FeatureId });

        builder.HasOne(pf => pf.Project)
            .WithMany(p => p.ProjectFeatures)
            .HasForeignKey(pf => pf.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pf => pf.Feature)
            .WithMany(f => f.ProjectFeatures)
            .HasForeignKey(pf => pf.FeatureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

