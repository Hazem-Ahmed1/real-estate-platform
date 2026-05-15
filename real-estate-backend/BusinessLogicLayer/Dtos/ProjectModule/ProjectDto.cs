using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Dtos.ProjectModule;

public class ProjectDto
{
    [Required(ErrorMessage = "Project name is required.")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Project name must be between 1 and 255 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "City must not exceed 100 characters.")]
    public string? City { get; set; }

    [StringLength(100, ErrorMessage = "Area must not exceed 100 characters.")]
    public string? Area { get; set; }

    [StringLength(255, ErrorMessage = "Address must not exceed 255 characters.")]
    public string? Address { get; set; }

    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double? Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
    public double? Longitude { get; set; }


    [Range(0, double.MaxValue, ErrorMessage = "TotalArea must be greater than or equal to 0.")]
    public double? TotalArea { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "LandArea must be greater than or equal to 0.")]
    public double? LandArea { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "BuildUpArea must be greater than or equal to 0.")]
    public double? BuildUpArea { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "TotalBuildingArea must be greater than or equal to 0.")]
    public double? TotalBuildingArea { get; set; }

    [Required(ErrorMessage = "Project status is required.")]
    public ProjectStatus Status { get; set; } = ProjectStatus.Sale;

    public bool IsStatusChanged { get; set; }

    public List<int> FeatureIds { get; set; } = new();
    public List<int> InsuranceIds { get; set; } = new();

    public List<int> DeletedMediaIds { get; set; } = new();

    public IFormFile? ThumbnailImage { get; set; }
    public List<IFormFile> Images { get; set; } = new();
    public IFormFile? Video { get; set; }
    public IFormFile? Panorama360 { get; set; }
}
