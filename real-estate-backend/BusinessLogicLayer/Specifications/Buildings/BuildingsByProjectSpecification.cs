namespace BusinessLogicLayer.Specifications.Buildings;

public class BuildingsByProjectSpecification : BaseSpecifications<Building>
{
    public BuildingsByProjectSpecification(int? projectId = null, bool publicOnly = false)
        : base(x => 
            (!projectId.HasValue || x.ProjectId == projectId) &&
            (!publicOnly || (x.Project.Status == ProjectStatus.Sale || x.Project.Status == ProjectStatus.Rent))
        )
    {
        AddInclude(x => x.Project);
    }

}
