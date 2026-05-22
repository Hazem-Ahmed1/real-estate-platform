namespace DataAccessLayer.Entities.UnitModule;

public class UnitInsurance: BaseEntity
{
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public int InsuranceId { get; set; }
    public Insurance Insurance { get; set; } = null!;
}




