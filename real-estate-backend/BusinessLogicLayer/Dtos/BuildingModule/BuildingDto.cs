using BusinessLogicLayer.Dtos.UnitModule;

namespace BusinessLogicLayer.Dtos.BuildingModule;

public class BuildingDto
{
    public int BuildingId { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Name { get; set; } = null!;
    public double MaxArea { get; set; }
    public double BuildingArea { get; set; }
    public int? FloorCount { get; set; }
    public List<UnitListDto> Units { get; set; } = new();
}
