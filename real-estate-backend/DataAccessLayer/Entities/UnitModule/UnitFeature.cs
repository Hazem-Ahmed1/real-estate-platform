using DataAccessLayer.Entities.LookupModule;
namespace DataAccessLayer.Entities.UnitModule;

public class UnitFeature:BaseEntity
{
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public int FeatureId { get; set; }
    public Feature Feature { get; set; } = null!;
}




