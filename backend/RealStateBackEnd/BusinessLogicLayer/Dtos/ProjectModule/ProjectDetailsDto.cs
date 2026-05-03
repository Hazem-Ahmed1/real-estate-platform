namespace BusinessLogicLayer.Dtos.ProjectModule;

public class ProjectDetailsDto
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int BuildingsCount { get; set; }
    public int UnitsCount { get; set; }
    public double? TotalArea { get; set; }
    public List<string> Features { get; set; } = new();
    public List<ProjectMediaDto> Media { get; set; } = new();
}
