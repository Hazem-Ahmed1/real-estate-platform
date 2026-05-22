namespace BusinessLogicLayer.Specifications.Buildings;

public class BuildingsByProjectSpecification : BaseSpecifications<Building>
{
    public BuildingsByProjectSpecification(int? projectId = null)
        : base(x => 
            (!projectId.HasValue || x.ProjectId == projectId)
        )
    {
        AddInclude(x => x.Project);
        AddInclude(x => x.Units);
    }

}
