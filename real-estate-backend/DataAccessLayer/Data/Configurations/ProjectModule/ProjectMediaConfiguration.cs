

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Data.Configurations;

public class ProjectMediaConfiguration : IEntityTypeConfiguration<ProjectMedia>
{
    public void Configure(EntityTypeBuilder<ProjectMedia> builder)
    {
        builder.ToTable("Project_Media");
        builder.HasKey(pm => pm.MediaId);
        
        builder.Property(pm => pm.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(pm => pm.MediaUrl).IsRequired();

        builder.HasIndex(pm => pm.ProjectId);
        builder.HasIndex(pm => new { pm.ProjectId, pm.IsThumbnail });
    }
}

