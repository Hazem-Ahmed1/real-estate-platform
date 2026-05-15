using BusinessLogicLayer.Specifications;

namespace BusinessLogicLayer.Specifications.Buildings;

public class BuildingWithProjectSpecification : BaseSpecifications<Building>
{
    public BuildingWithProjectSpecification(int id) : base(b => b.BuildingId == id)
    {
        AddInclude(b => b.Project);
    }
}
