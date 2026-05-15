using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace DataAccessLayer.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(p => p.ProjectId);
        
        builder.Property(p => p.Name).IsRequired().HasMaxLength(255);
        builder.HasIndex(p => p.Name).IsUnique();
        
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(p => p.City).HasMaxLength(100);
        builder.Property(p => p.Area).HasMaxLength(100);
        builder.Property(p => p.Address).HasMaxLength(255);


        builder.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");

        // One-to-Many: Project -> Buildings
        builder.HasMany(p => p.Buildings)
            .WithOne(b => b.Project)
            .HasForeignKey(b => b.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-Many: Project -> Media
        builder.HasMany(p => p.Media)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

