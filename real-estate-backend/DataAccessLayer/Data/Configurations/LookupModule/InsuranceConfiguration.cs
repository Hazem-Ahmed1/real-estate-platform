

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Data.Configurations;

public class InsuranceConfiguration : IEntityTypeConfiguration<Insurance>
{
    public void Configure(EntityTypeBuilder<Insurance> builder)
    {
        builder.ToTable("Insurance");
        builder.HasKey(i => i.InsuranceId);
        builder.Property(i => i.Name).IsRequired().HasMaxLength(255);
        builder.HasIndex(i => i.Name).IsUnique();

    }
}

