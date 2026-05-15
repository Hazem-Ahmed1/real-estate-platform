
namespace BusinessLogicLayer.Dtos.UnitModule;

public class UnitListDto
{
    public int UnitId { get; set; }
    public string? ProjectName { get; set; }
    public string? BuildingName { get; set; }
    public UnitType Type { get; set; }
    public UnitStatus Status { get; set; }
    public bool? IsStatusChanged { get; set; }

    public decimal Price { get; set; }
    public double Area { get; set; }
    public int Rooms { get; set; }
    public int Salons { get; set; }
    public int StreetCount { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Street { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ThumbnailUrl { get; set; }
}
