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

public class UnitInsuranceConfiguration : IEntityTypeConfiguration<UnitInsurance>
{
    public void Configure(EntityTypeBuilder<UnitInsurance> builder)
    {
        builder.ToTable("Unit_Insurance");
        builder.HasKey(ui => new { ui.UnitId, ui.InsuranceId });

        builder.HasOne(ui => ui.Unit)
            .WithMany(u => u.UnitInsurance)
            .HasForeignKey(ui => ui.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ui => ui.Insurance)
            .WithMany(i => i.UnitInsurance)
            .HasForeignKey(ui => ui.InsuranceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

