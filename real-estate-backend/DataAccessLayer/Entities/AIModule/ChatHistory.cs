using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entities.AIModule;

public class ChatHistory : AuditableEntity
{
    public int ChatId { get; set; }

    public string? SessionId { get; set; }
    public int? UserId { get; set; }

    public string UserMessage { get; set; } = null!;

    public string AiResponse { get; set; } = null!;
}
