using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entities.LookupModule;

public class Insurance : BaseEntity
{
    public int InsuranceId { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<ProjectInsurance> ProjectInsurance { get; set; } = new List<ProjectInsurance>();
    public ICollection<UnitInsurance> UnitInsurance { get; set; } = new List<UnitInsurance>();
}
