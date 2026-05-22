
namespace BusinessLogicLayer.Specifications.Projects;

public class ProjectWithBuildingsSpecification : BaseSpecifications<Project>
{
    public ProjectWithBuildingsSpecification(ProjectSpecParams @params, bool isCount = false)
        : base(p => 
            (string.IsNullOrEmpty(@params.City) || (p.City != null && p.City.ToLower() == @params.City.ToLower())) &&
            (!@params.Status.HasValue || p.Status == @params.Status) &&
            (!@params.UnitStatus.HasValue || p.Buildings.Any(b => b.Units.Any(u => u.Status == @params.UnitStatus))) &&
            (!@params.Type.HasValue || p.Buildings.Any(b => b.Units.Any(u => u.Type == @params.Type))) &&
            (!@params.Rooms.HasValue || p.Buildings.Any(b => b.Units.Any(u => u.Rooms == @params.Rooms))) &&
            (!@params.MinPrice.HasValue && !@params.MaxPrice.HasValue || p.Buildings.Any(b => b.Units.Any(u => 
                (!@params.MinPrice.HasValue || u.Price >= @params.MinPrice) && 
                (!@params.MaxPrice.HasValue || u.Price <= @params.MaxPrice)))) &&
            (string.IsNullOrEmpty(@params.Search) || 
                p.Name.Contains(@params.Search))
        )
    {
        if (!isCount)
        {
            AddInclude("Buildings");
            AddInclude("Buildings.Units");
            AddInclude("Media");
            AddInclude("ProjectFeatures.Feature");
            AddInclude("ProjectInsurance.Insurance");
            
            AddOrderByDescending(p => p.CreatedAt); // الافتراضي هو الأحدث
            
            ApplyPagination(@params.PageSize, @params.Page);
        }
    }

    public ProjectWithBuildingsSpecification(int id) : base(p => p.ProjectId == id)
    {
        AddInclude("Media");
        AddInclude("ProjectFeatures.Feature");
        AddInclude("ProjectInsurance.Insurance");
    }
}
