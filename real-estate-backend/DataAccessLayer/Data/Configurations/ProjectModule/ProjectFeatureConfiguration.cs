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

public class ProjectFeatureConfiguration : IEntityTypeConfiguration<ProjectFeature>
{
    public void Configure(EntityTypeBuilder<ProjectFeature> builder)
    {
        builder.ToTable("Project_Features");
        builder.HasKey(pf => new { pf.ProjectId, pf.FeatureId });

        builder.HasOne(pf => pf.Project)
            .WithMany(p => p.ProjectFeatures)
            .HasForeignKey(pf => pf.ProjectId);

        builder.HasOne(pf => pf.Feature)
            .WithMany(f => f.ProjectFeatures)
            .HasForeignKey(pf => pf.FeatureId);
    }
}

