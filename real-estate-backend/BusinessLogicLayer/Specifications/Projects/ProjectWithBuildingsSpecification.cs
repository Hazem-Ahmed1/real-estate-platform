
namespace BusinessLogicLayer.Specifications.Projects;

public class ProjectWithBuildingsSpecification : BaseSpecifications<Project>
{
    public ProjectWithBuildingsSpecification(ProjectSpecParams @params, bool isCount = false)
        : base(p => 
            (!@params.PublicOnly || (p.Status == ProjectStatus.Sale || p.Status == ProjectStatus.Rent)) &&


            (!@params.Status.HasValue || p.Status == @params.Status) &&
            (!@params.UnitStatus.HasValue || p.Buildings.Any(b => b.Units.Any(u => u.Status == @params.UnitStatus))) &&
            (!@params.MinPrice.HasValue && !@params.MaxPrice.HasValue || p.Buildings.Any(b => b.Units.Any(u => 
                (!@params.MinPrice.HasValue || u.Price >= @params.MinPrice) && 
                (!@params.MaxPrice.HasValue || u.Price <= @params.MaxPrice)))) &&
            (string.IsNullOrEmpty(@params.Search) || 
                p.Name.Contains(@params.Search))
        )
    {
        if (!isCount)
        {
            AddInclude(p => p.Buildings);
            AddInclude("Buildings.Units");
            AddInclude(p => p.Media);
            AddInclude("ProjectFeatures.Feature");
            AddInclude("ProjectInsurance.Insurance");
            
            AddOrderByDescending(p => p.CreatedAt); // الافتراضي هو الأحدث
            
            ApplyPagination(@params.PageSize, @params.Page);
        }
    }

    public ProjectWithBuildingsSpecification(int id) : base(p => p.ProjectId == id)
    {
        AddInclude(p => p.Buildings);
        AddInclude("Buildings.Units");
        AddInclude(p => p.Media);
        AddInclude("ProjectFeatures.Feature");
        AddInclude("ProjectInsurance.Insurance");
    }
}
