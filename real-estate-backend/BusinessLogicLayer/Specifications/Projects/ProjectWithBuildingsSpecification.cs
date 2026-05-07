using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Enums;
using System.Linq;

namespace BusinessLogicLayer.Specifications.Projects;

public class ProjectWithBuildingsSpecification : BaseSpecifications<Project>
{
    public ProjectWithBuildingsSpecification(ProjectSpecParams @params, bool isCount = false)
        : base(p => 
            (!@params.Status.HasValue || p.Status == @params.Status) &&
            (!@params.UnitStatus.HasValue || p.Buildings.Any(b => b.Units.Any(u => u.Status == @params.UnitStatus))) &&
            (string.IsNullOrEmpty(@params.City) || p.City != null && p.City.Contains(@params.City)) &&
            (string.IsNullOrEmpty(@params.Search) || 
                p.Name.Contains(@params.Search) || 
                (p.Description != null && p.Description.Contains(@params.Search)))
        )
    {
        if (!isCount)
        {
            AddInclude(p => p.Buildings);
            AddInclude("Buildings.Units");
            AddInclude(p => p.Media);
            AddInclude("ProjectFeatures.Feature");
            AddInclude("ProjectInsurance.Insurance");
            
            if (!string.IsNullOrEmpty(@params.Sort))
            {
                switch (@params.Sort)
                {
                    case "dateAsc":
                        AddOrderBy(p => p.CreatedAt);
                        break;
                    case "dateDesc":
                        AddOrderByDescending(p => p.CreatedAt);
                        break;
                    case "priceAsc":
                        // الترتيب حسب أقل سعر وحدة في المشروع
                        AddOrderBy(p => p.Buildings.SelectMany(b => b.Units).Min(u => u.Price) ?? 0);
                        break;
                    case "priceDesc":
                        // الترتيب حسب أعلى سعر وحدة في المشروع
                        AddOrderByDescending(p => p.Buildings.SelectMany(b => b.Units).Max(u => u.Price) ?? 0);
                        break;
                    case "nameDesc":
                        AddOrderByDescending(p => p.Name);
                        break;
                    default:
                        AddOrderBy(p => p.Name);
                        break;
                }
            }
            else
            {
                AddOrderByDescending(p => p.CreatedAt); // الافتراضي هو الأحدث
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
        AddInclude("ProjectInsurance.Insurance");
    }
}
