using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entities.LookupModule;

public class Feature : BaseEntity
{
    public int FeatureId { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<ProjectFeature> ProjectFeatures { get; set; } = new List<ProjectFeature>();
    public ICollection<UnitFeature> UnitFeatures { get; set; } = new List<UnitFeature>();
}
