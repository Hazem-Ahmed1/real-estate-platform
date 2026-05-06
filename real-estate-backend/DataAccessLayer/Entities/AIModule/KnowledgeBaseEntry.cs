using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entities.AIModule;

public class KnowledgeBaseEntry : AuditableEntity
{
    public int EntryId { get; set; }

    public KnowledgeEntityType EntityType { get; set; }
    public int? EntityId { get; set; }

    public string Content { get; set; } = null!;

    public string VectorData { get; set; } = null!; // OpenAI Embedding (JSON)
}
