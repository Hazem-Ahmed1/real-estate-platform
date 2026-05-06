namespace BusinessLogicLayer.Dtos.ProjectModule;

public class ProjectListDto
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public int BuildingsCount { get; set; }
    public int UnitsCount { get; set; }
    public double? TotalArea { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
}
