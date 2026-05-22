using DataAccessLayer.Enums;

namespace BusinessLogicLayer.Dtos.UnitModule;

public class GetUnitDto
{
    public int UnitId { get; set; }
    public string Name { get; set; } = null!;
    public int BuildingId { get; set; }
    
    public string? BuildingName { get; set; }
    public string? ProjectName { get; set; }
    
    public int? Rooms { get; set; }
    
    public int? Salons { get; set; }
    
    public double? Area { get; set; }
    
    public int? Bathrooms { get; set; }
    
    public int? Floor { get; set; }
    
    public decimal? Price { get; set; }

    public UnitType Type { get; set; }
    public UnitStatus Status { get; set; }

    public int StreetCount { get; set; }

    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Street { get; set; }
    public string? Address { get; set; }
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    public string? ThumbnailUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? PanoramaUrl { get; set; }

    public List<string> Images { get; set; } = new();
    public List<string> Designs { get; set; } = new();

    public List<NearbyFacilityDto> NearbyFacilities { get; set; } = new();
    public List<FeatureDto> Features { get; set; } = new();
    public List<InsuranceDto> Insurance { get; set; } = new();
}
