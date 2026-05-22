
namespace DataAccessLayer.Entities.LookupModule;

public class NearbyFacility : AuditableEntity
{
    public int FacilityId { get; set; }

    public int? UnitId { get; set; }
    public Unit? Unit { get; set; }

    public FacilityType Type { get; set; }
    
    public double Area { get; set; }

    public string Name { get; set; } = null!;

    public string? Distance { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
