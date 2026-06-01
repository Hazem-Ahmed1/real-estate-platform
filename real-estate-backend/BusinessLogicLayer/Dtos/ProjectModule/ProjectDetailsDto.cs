
namespace BusinessLogicLayer.Dtos.ProjectModule;

public class ProjectDetailsDto
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }

    public string CityName { get; set; } = null!;
    public string? RegionName { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double BuildUpArea { get; set; }
    public List<BuildingDto> Buildings { get; set; } = new();

    public int BuildingsNumber { get; set; }
    public int UnitsNumber { get; set; }
    public int AvailableUnitsCount { get; set; }
    public int TransactedUnitsCount { get; set; }
    public int TotalRooms { get; set; }
    public int TotalHalls { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public double? TotalBuildingArea { get; set; }
    public double TotalFacilitiesArea { get; set; }
    
    public string? ThumbnailUrl { get; set; }
    public string? Panorama360Url { get; set; }
    public string? VideoUrl { get; set; }
    public List<string> Images { get; set; } = new();
    public List<ProjectMediaDto> Media { get; set; } = new();

    public List<FeatureDto> Features { get; set; } = new();
    public List<InsuranceDto> Insurance { get; set; } = new();
}
