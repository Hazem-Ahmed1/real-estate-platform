
namespace DataAccessLayer.Entities.UnitModule;

public class UnitMedia : AuditableEntity
{
    public int MediaId { get; set; }

    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public MediaType Type { get; set; }

    public string MediaUrl { get; set; } = null!;

    public string? ThumbnailUrl { get; set; }

    public bool IsThumbnail { get; set; }
    public string? PublicId { get; set; }
}
