using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Enums;
namespace DataAccessLayer.Entities.ProjectModule;

public class ProjectInsurance: BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int InsuranceId { get; set; }
    public Insurance Insurance { get; set; } = null!;
}




