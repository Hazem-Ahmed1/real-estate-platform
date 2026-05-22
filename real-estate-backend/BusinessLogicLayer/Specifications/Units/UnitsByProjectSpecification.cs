using DataAccessLayer.Entities.UnitModule;

namespace BusinessLogicLayer.Specifications.Units;

public class UnitsByProjectSpecification : BaseSpecifications<Unit>
{
    public UnitsByProjectSpecification(int projectId) : base(u => u.Building.ProjectId == projectId)
    {
        AddInclude(u => u.Building);
        AddInclude("Building.Project");
        AddInclude(u => u.Media);
    }
}
