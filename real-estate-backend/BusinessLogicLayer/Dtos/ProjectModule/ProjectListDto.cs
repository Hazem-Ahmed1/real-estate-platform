namespace BusinessLogicLayer.Dtos.ProjectModule;

public class ProjectListDto
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public bool? IsStatusChanged { get; set; }

    public string City { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public int BuildingsNumber { get; set; }
    public int UnitsNumber { get; set; }
    public int AvailableUnitsCount { get; set; }
    public int TransactedUnitsCount { get; set; }
    public int TotalRooms { get; set; }
    public int TotalHalls { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public double? TotalBuildingArea { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
}
