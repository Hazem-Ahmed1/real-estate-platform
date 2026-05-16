using DataAccessLayer.Enums;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Dtos.UnitModule;

public class UnitCreateDto
{
    public string Name { get; set; } = null!;
    public int BuildingId { get; set; }
    public int Rooms { get; set; }
    public int Salons { get; set; }
    public double Area { get; set; }
    public int Bathrooms { get; set; }
    public int Floor { get; set; }
    public decimal Price { get; set; }
    public UnitType Type { get; set; }
    public UnitStatus Status { get; set; }
    public int StreetCount { get; set; }
    public string Street { get; set; } = null!;
    public double Latitude { get; set; }
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
