using Microsoft.AspNetCore.Http;
using DataAccessLayer.Enums;
using System.ComponentModel.DataAnnotations;

namespace APILayer.Dtos.Units;

public class UnitCreateFormDto
{
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public int BuildingId { get; set; }
    [Required(ErrorMessage = "Rooms count is required.")]
    [Range(1, 100, ErrorMessage = "Rooms must be at least 1.")]
    public int Rooms { get; set; }

    [Required(ErrorMessage = "Salons count is required.")]
    [Range(1, 100, ErrorMessage = "Salons must be at least 1.")]
    public int Salons { get; set; }

    [Required(ErrorMessage = "Area is required.")]
    [Range(10, 100000, ErrorMessage = "Area must be at least 10.")]
    public double Area { get; set; }

    [Required(ErrorMessage = "Bathrooms count is required.")]
    [Range(1, 100, ErrorMessage = "Bathrooms must be at least 1.")]
    public int Bathrooms { get; set; }

    [Required(ErrorMessage = "Floor number is required.")]
    [Range(1, 1000, ErrorMessage = "Floor must be at least 1.")]
    public int Floor { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(50, double.MaxValue, ErrorMessage = "Price must be at least 50.")]
    public decimal Price { get; set; }

    public UnitType Type { get; set; }
    public UnitStatus Status { get; set; } = UnitStatus.Sale;

    [Required(ErrorMessage = "Street count is required.")]
    [Range(1, 4, ErrorMessage = "Street count must be between 1 and 4.")]
    public int StreetCount { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(200)]
    public string? Region { get; set; }

    [StringLength(200)]
    public string? Street { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(500, MinimumLength = 3)]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "Latitude is required.")]
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Required(ErrorMessage = "Longitude is required.")]
    [Range(-180, 180)]
    public double Longitude { get; set; }
    public List<int> FeatureIds { get; set; } = new();
    public List<int> InsuranceIds { get; set; } = new();
    public List<NearbyFacilityDto> NearbyFacilities { get; set; } = new();

    public IFormFile? ThumbnailImage { get; set; }
    public List<IFormFile>? Images { get; set; }
    public List<IFormFile>? Designs { get; set; }
    public IFormFile? VideoFile { get; set; }
    public IFormFile? Panorama360 { get; set; }
}
