using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Enums;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Dtos.ProjectModule;

public class ProjectCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string City { get; set; } = string.Empty;
    public string? Region { get; set; } = string.Empty;

    public string? Address { get; set; } = string.Empty;
    [Range(-90, 90)]
    public double Latitude { get; set; }
    [Range(-180, 180)]
    public double Longitude { get; set; }
    [Range(1, 10000000)]
    public double LandArea { get; set; }
    [Range(1, 10000000)]
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
