using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entities.LookupModule;

public class NearbyFacility : AuditableEntity
{
    public int FacilityId { get; set; }

    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public FacilityType Type { get; set; }

    public string Name { get; set; } = null!;

    public string? Distance { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
