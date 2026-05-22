using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Enums;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Dtos.UnitModule;

public class UnitCreateDto
{
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public int BuildingId { get; set; }
    [Range(1, 50)]
    public int Rooms { get; set; }
    [Range(0, 50)]
    public int Salons { get; set; }
    [Range(1, 1000000)]
    public double Area { get; set; }
    [Range(1, 50)]
    public int Bathrooms { get; set; }
    [Range(0, 1000)]
    public int Floor { get; set; }
    [Range(1, 1000000000)]
    public decimal Price { get; set; }
    public UnitType Type { get; set; }
    public UnitStatus Status { get; set; }
    [Range(1, 4)]
    public int StreetCount { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Street { get; set; }
    public string? Address { get; set; }
    [Range(-90, 90)]
    public double Latitude { get; set; }
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
