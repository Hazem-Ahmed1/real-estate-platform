using BusinessLogicLayer.Specifications;

namespace BusinessLogicLayer.Specifications.Buildings;

public class BuildingWithProjectSpecification : BaseSpecifications<Building>
{
    public BuildingWithProjectSpecification() : base()
    {
        AddInclude(b => b.Project);
        AddInclude(b => b.Units);
    }

    public BuildingWithProjectSpecification(int id) : base(b => b.BuildingId == id)
    {
        AddInclude(b => b.Project);
        AddInclude(b => b.Units);
    }
}
