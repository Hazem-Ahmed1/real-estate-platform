

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Data.Configurations;

public class ProjectInsuranceConfiguration : IEntityTypeConfiguration<ProjectInsurance>
{
    public void Configure(EntityTypeBuilder<ProjectInsurance> builder)
    {
        builder.ToTable("Project_Insurance");
        builder.HasKey(pi => new { pi.ProjectId, pi.InsuranceId });

        builder.HasOne(pi => pi.Project)
            .WithMany(p => p.ProjectInsurance)
            .HasForeignKey(pi => pi.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pi => pi.Insurance)
            .WithMany(i => i.ProjectInsurance)
            .HasForeignKey(pi => pi.InsuranceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

