using Microsoft.AspNetCore.Http;
using DataAccessLayer.Enums;
using System.ComponentModel.DataAnnotations;

namespace APILayer.Dtos.Projects;

public class ProjectCreateFormDto
{
    [Required(ErrorMessage = "Project name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string City { get; set; } = null!;

    [StringLength(200)]
    public string? Region { get; set; } = null!;



    [StringLength(500)]
    public string? Address { get; set; } = null!;

    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public double LandArea { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public double BuildUpArea { get; set; }

    public double? TotalBuildingArea { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Sale;
    public List<int> FeatureIds { get; set; } = new();
    public List<int> InsuranceIds { get; set; } = new();

    public IFormFile? ThumbnailImage { get; set; }
    public List<IFormFile>? Images { get; set; }
    public IFormFile? VideoFile { get; set; }
    public IFormFile? Panorama360 { get; set; }
}
