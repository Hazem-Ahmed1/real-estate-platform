using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Enums;
using System.Linq;

namespace BusinessLogicLayer.Specifications.Projects;

public class ProjectWithBuildingsSpecification : BaseSpecifications<Project>
{
    public ProjectWithBuildingsSpecification(ProjectSpecParams @params, bool isCount = false)
        : base(p => 
            (!@params.Status.HasValue || p.Status == @params.Status) &&
            (string.IsNullOrEmpty(@params.City) || p.City != null && p.City.Contains(@params.City)) &&
            (string.IsNullOrEmpty(@params.Search) || p.Name.Contains(@params.Search))
        )
    {
        if (!isCount)
        {
            AddInclude(p => p.Buildings);
            AddInclude("Buildings.Units");
            AddInclude(p => p.Media);
            AddInclude("ProjectFeatures.Feature");
            
            if (!string.IsNullOrEmpty(@params.Sort))
            {
                switch (@params.Sort)
                {
                    case "nameDesc":
                        AddOrderByDescending(p => p.Name);
                        break;
                    case "status":
                        AddOrderBy(p => p.Status);
                        break;
                    default:
                        AddOrderBy(p => p.Name);
                        break;
                }
            }
            else
            {
                AddOrderBy(p => p.Name);
            }

            ApplyPagination(@params.PageSize, @params.Page);
        }
    }

    public ProjectWithBuildingsSpecification(int id) : base(p => p.ProjectId == id)
    {
        AddInclude(p => p.Buildings);
        AddInclude("Buildings.Units");
        AddInclude(p => p.Media);
        AddInclude("ProjectFeatures.Feature");
    }
}
