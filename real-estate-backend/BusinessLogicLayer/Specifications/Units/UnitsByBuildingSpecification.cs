
namespace BusinessLogicLayer.Specifications.Units;

public class UnitsByBuildingSpecification : BaseSpecifications<Unit>
{
    public UnitsByBuildingSpecification(int buildingId) : base(u => u.BuildingId == buildingId)
    {
    }
}
