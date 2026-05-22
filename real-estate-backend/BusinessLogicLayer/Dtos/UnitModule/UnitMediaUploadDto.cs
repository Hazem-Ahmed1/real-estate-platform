using DataAccessLayer.Enums;

namespace BusinessLogicLayer.Dtos.UnitModule;

public class UnitMediaUploadDto
{
    public string Url { get; set; } = null!;
    public string? PublicId { get; set; }
    public MediaType Type { get; set; }
    public bool IsThumbnail { get; set; }
}
