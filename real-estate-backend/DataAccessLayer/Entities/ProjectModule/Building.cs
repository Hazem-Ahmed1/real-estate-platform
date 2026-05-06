using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entities.ProjectModule;

public class Building : AuditableEntity
{
    public int BuildingId { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string? Name { get; set; }
    public int? Floors { get; set; }

    public ICollection<Unit> Units { get; set; } = new List<Unit>();
}
