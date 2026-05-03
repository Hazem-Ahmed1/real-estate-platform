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

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("Blog");
        builder.HasKey(b => b.BlogId);
        builder.Property(b => b.Title).IsRequired().HasMaxLength(255);
        builder.Property(b => b.PublishDate).HasDefaultValueSql("GETDATE()");
        builder.HasIndex(b => b.PublishDate);

        // One-to-Many: BlogPost -> Images
        builder.HasMany(b => b.Images)
            .WithOne(i => i.Blog)
            .HasForeignKey(i => i.BlogId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

