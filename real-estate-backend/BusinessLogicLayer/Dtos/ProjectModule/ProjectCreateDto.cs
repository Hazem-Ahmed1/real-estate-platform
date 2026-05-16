using DataAccessLayer.Enums;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Dtos.ProjectModule;

public class ProjectCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Area { get; set; } = string.Empty;

    public string? Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double LandArea { get; set; }
    public double BuildUpArea { get; set; }
    public double? TotalBuildingArea { get; set; }
    public ProjectStatus Status { get; set; }
    public List<int> FeatureIds { get; set; } = new();
    public List<int> InsuranceIds { get; set; } = new();
    public IFormFile? ThumbnailImage { get; set; }
    public List<IFormFile>? Images { get; set; }
    public IFormFile? VideoFile { get; set; }
    public IFormFile? Panorama360 { get; set; }
}
