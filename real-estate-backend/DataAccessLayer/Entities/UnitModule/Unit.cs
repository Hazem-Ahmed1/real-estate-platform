using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entities.UnitModule;

public class Unit : AuditableEntity
{
    public int UnitId { get; set; }
    public int BuildingId { get; set; }
    public Building Building { get; set; } = null!;

    public int? Rooms { get; set; }
    public int? Salons { get; set; }
    public double? Area { get; set; }
    public int? Bathrooms { get; set; }
    public int? Floor { get; set; }
    public decimal? Price { get; set; }

    public UnitType Type { get; set; }
    public UnitStatus Status { get; set; }

    public string? Street { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public ICollection<UnitMedia> Media { get; set; } = new List<UnitMedia>();
    public ICollection<UnitFeature> UnitFeatures { get; set; } = new List<UnitFeature>();
    public ICollection<NearbyFacility> NearbyFacilities { get; set; } = new List<NearbyFacility>();
    public ICollection<UnitInsurance> UnitInsurance { get; set; } = new List<UnitInsurance>();
}
