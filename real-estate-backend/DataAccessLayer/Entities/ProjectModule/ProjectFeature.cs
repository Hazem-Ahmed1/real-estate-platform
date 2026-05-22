namespace DataAccessLayer.Entities.ProjectModule;

public class ProjectFeature:BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int FeatureId { get; set; }
    public Feature Feature { get; set; } = null!;
}




