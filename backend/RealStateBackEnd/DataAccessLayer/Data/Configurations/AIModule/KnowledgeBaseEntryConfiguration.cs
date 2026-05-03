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

public class KnowledgeBaseEntryConfiguration : IEntityTypeConfiguration<KnowledgeBaseEntry>
{
    public void Configure(EntityTypeBuilder<KnowledgeBaseEntry> builder)
    {
        builder.ToTable("Knowledge_Base");
        builder.HasKey(k => k.EntryId);

        builder.Property(k => k.EntityType).HasConversion<string>().HasMaxLength(50);
        builder.Property(k => k.Content).IsRequired();
        builder.Property(k => k.VectorData).IsRequired();

        builder.HasIndex(k => new { k.EntityType, k.EntityId });
    }
}

