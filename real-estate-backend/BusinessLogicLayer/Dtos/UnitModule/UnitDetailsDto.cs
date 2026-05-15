using System.ComponentModel.DataAnnotations;

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
    public bool? IsStatusChanged { get; set; }

    public int StreetCount { get; set; }

    public string? Street { get; set; }
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }


    public List<UnitMediaDto> Media { get; set; } = new();
    public List<NearbyFacilityDto> NearbyFacilities { get; set; } = new();
    public List<FeatureDto> Features { get; set; } = new();
    public List<InsuranceDto> Insurance { get; set; } = new();
}

public class NearbyFacilityDto
{
    [Required(ErrorMessage = "Facility name is required.")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Facility name must be between 1 and 255 characters.")]
    public string Name { get; set; } = null!;
    
    [Required(ErrorMessage = "Facility type is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Facility type must be between 1 and 100 characters.")]
    public string Type { get; set; } = null!;
    
    [Required(ErrorMessage = "Distance is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Distance must be between 1 and 50 characters.")]
    public string Distance { get; set; } = null!;
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
