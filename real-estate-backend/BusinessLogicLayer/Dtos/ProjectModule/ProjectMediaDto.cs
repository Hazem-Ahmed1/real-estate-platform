namespace BusinessLogicLayer.Dtos.ProjectModule;

public class ProjectMediaDto
{
    public int MediaId { get; set; }
    public int ProjectId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public bool IsThumbnail { get; set; }
}
