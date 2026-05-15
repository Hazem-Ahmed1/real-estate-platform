
namespace DataAccessLayer.Entities.LookupModule;

public class Feature : BaseEntity
{
    public int FeatureId { get; set; }

    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    public ICollection<ProjectFeature> ProjectFeatures { get; set; } = new List<ProjectFeature>();
    public ICollection<UnitFeature> UnitFeatures { get; set; } = new List<UnitFeature>();
}
