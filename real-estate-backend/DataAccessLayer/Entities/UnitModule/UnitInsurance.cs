using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Enums;
namespace DataAccessLayer.Entities.UnitModule;

public class UnitInsurance
{
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public int InsuranceId { get; set; }
    public Insurance Insurance { get; set; } = null!;
}




