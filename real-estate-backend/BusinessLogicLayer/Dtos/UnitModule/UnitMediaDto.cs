namespace BusinessLogicLayer.Dtos.UnitModule;

public class UnitMediaDto
{
    public int MediaId { get; set; }
    public int UnitId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public bool IsThumbnail { get; set; }
}
