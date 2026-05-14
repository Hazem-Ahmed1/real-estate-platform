using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Data.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(m => m.MessageId);

        builder.Property(m => m.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(m => m.FullName).IsRequired().HasMaxLength(255);
        builder.Property(m => m.Email).HasMaxLength(255);
        builder.Property(m => m.Phone).HasMaxLength(20);
        builder.Property(m => m.Subject).HasMaxLength(255);
        builder.Property(m => m.MessageBody).IsRequired();

        builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETDATE()");
        builder.HasIndex(m => m.Email);

        // No entity relationships for messages.
    }
}
