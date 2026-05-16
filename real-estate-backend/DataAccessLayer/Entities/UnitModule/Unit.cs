
namespace DataAccessLayer.Entities.UnitModule;

public class Unit : AuditableEntity
{
    public int UnitId { get; set; }
    public string? Name { get; set; } = null!;
    public int BuildingId { get; set; }
    public Building Building { get; set; } = null!;

    [Required]
    [RangeAttribute(1, 100)]
    public int Rooms { get; set; }

    [Required]
    [RangeAttribute(1, 100)]
    public int Salons { get; set; }

    [Required]
    [RangeAttribute(100, 1000000)]
    public double Area { get; set; }

    [Required]
    [RangeAttribute(1, 100)]
    public int Bathrooms { get; set; }

    [Required]
    [RangeAttribute(1, 1000)]
    public int Floor { get; set; }

    [Required]
    [RangeAttribute(50, 1000000000)]
    public decimal Price { get; set; }

    public UnitType Type { get; set; }
    public UnitStatus Status { get; set; }
    [Required]
    [RangeAttribute(1, 10)]
    public int StreetCount { get; set; }

    public bool IsStatusChanged { get; set; }

    [Required]
    [StringLength(500)]
    public string Street { get; set; } = null!;

    [Required]
    [RangeAttribute(-90, 90)]
    public double Latitude { get; set; }

    [Required]
    [RangeAttribute(-180, 180)]
    public double Longitude { get; set; }

    public ICollection<UnitMedia> Media { get; set; } = new List<UnitMedia>();
    public ICollection<UnitFeature> UnitFeatures { get; set; } = new List<UnitFeature>();
    public ICollection<NearbyFacility> NearbyFacilities { get; set; } = new List<NearbyFacility>();
    public ICollection<UnitInsurance> UnitInsurance { get; set; } = new List<UnitInsurance>();
}
