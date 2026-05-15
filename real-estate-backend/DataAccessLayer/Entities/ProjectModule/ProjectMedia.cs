
namespace DataAccessLayer.Entities.ProjectModule;

public class ProjectMedia : AuditableEntity
{
    public int MediaId { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public MediaType Type { get; set; }

    public string MediaUrl { get; set; } = null!;

    public string? ThumbnailUrl { get; set; }

    public bool IsThumbnail { get; set; }
    public string? PublicId { get; set; }
}
