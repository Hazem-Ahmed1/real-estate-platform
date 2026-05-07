using BusinessLogicLayer.Dtos.ProjectModule;
using DataAccessLayer.Enums;

namespace BusinessLogicLayer.Dtos.ProjectModule;

public class GetProjectDto
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public bool IsDeleted { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int BuildingsCount { get; set; }
    public int UnitsCount { get; set; }
    public int AvailableUnitsCount { get; set; }
    public int TotalRooms { get; set; }
    public int TotalHalls { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public double? TotalArea { get; set; }
    
    public string? Panorama360Url { get; set; }
    public ProjectVideoDto? Video { get; set; }
    public List<ProjectMediaDto> Images { get; set; } = new();

    public List<string> Features { get; set; } = new();
    public List<string> Insurance { get; set; } = new();
}
