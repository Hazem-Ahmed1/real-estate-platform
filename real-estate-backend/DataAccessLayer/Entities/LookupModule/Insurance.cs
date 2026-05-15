
namespace DataAccessLayer.Entities.LookupModule;

public class Insurance : BaseEntity
{
    public int InsuranceId { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public ICollection<ProjectInsurance> ProjectInsurance { get; set; } = new List<ProjectInsurance>();
    public ICollection<UnitInsurance> UnitInsurance { get; set; } = new List<UnitInsurance>();
}
