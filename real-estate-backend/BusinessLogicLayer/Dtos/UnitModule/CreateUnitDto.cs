using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Dtos.UnitModule;

public class CreateUnitDto
{
    [Required]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Unit name must be between 1 and 200 characters.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "BuildingId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "BuildingId must be greater than 0.")]
    public int BuildingId { get; set; }

    [Required(ErrorMessage = "Rooms is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Rooms must be greater than 0.")]
    public int? Rooms { get; set; }

    [Required(ErrorMessage = "Salons is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Salons must be greater than 0.")]
    public int? Salons { get; set; }

    [Required(ErrorMessage = "Area is required.")]
    [Range(100, double.MaxValue, ErrorMessage = "Area must be at least 100.")]
    public double? Area { get; set; }

    [Required(ErrorMessage = "Bathrooms is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Bathrooms must be greater than 0.")]
    public int? Bathrooms { get; set; }

    [Required(ErrorMessage = "Floor is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Floor must be greater than 0.")]
    public int? Floor { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(50, double.MaxValue, ErrorMessage = "Price must be at least 50.")]
    public decimal? Price { get; set; }

    [Required(ErrorMessage = "Unit type is required.")]
    public UnitType Type { get; set; }

    public UnitStatus Status { get; set; } = UnitStatus.Sale;


    [Required(ErrorMessage = "StreetCount is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "StreetCount must be greater than 0.")]
    public int StreetCount { get; set; }

    [Required(ErrorMessage = "Street is required.")]
    [StringLength(255, ErrorMessage = "Street must not exceed 255 characters.")]
    public string? Street { get; set; }

    [Required(ErrorMessage = "Latitude is required.")]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double? Latitude { get; set; }

    [Required(ErrorMessage = "Longitude is required.")]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
    public double? Longitude { get; set; }

    public List<int> FeatureIds { get; set; } = new();
    public List<int> InsuranceIds { get; set; } = new();

    public IFormFile? ThumbnailImage { get; set; }
    public List<IFormFile> Images { get; set; } = new();
    public List<IFormFile> Designs { get; set; } = new();
    public IFormFile? Video { get; set; }
    public IFormFile? Panorama360 { get; set; }
}
