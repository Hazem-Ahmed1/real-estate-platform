
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.LookupModule;

public class Insurance : BaseEntity
{
    public int InsuranceId { get; set; }

    public string Name { get; set; } = null!;

    [Range(1, 100)]
    public int Duration { get; set; }

    public bool IsActive { get; set; } = true;
    public ICollection<ProjectInsurance> ProjectInsurance { get; set; } = new List<ProjectInsurance>();
    public ICollection<UnitInsurance> UnitInsurance { get; set; } = new List<UnitInsurance>();
}
