using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entities.CommunicationModule;

public class Message : AuditableEntity
{
    public int MessageId { get; set; }

    public MessageType Type { get; set; }

    public int? UnitId { get; set; }
    public Unit? Unit { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Subject { get; set; }

    public string MessageBody { get; set; } = null!;
}
