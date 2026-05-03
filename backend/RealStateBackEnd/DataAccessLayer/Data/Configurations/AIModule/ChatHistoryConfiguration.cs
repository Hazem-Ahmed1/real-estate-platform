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

public class ChatHistoryConfiguration : IEntityTypeConfiguration<ChatHistory>
{
    public void Configure(EntityTypeBuilder<ChatHistory> builder)
    {
        builder.ToTable("Chat_History");
        builder.HasKey(c => c.ChatId);
        
        builder.Property(c => c.UserMessage).IsRequired();
        builder.Property(c => c.AiResponse).IsRequired();

        builder.HasIndex(c => c.SessionId);
        builder.HasIndex(c => c.CreatedAt);
    }
}

