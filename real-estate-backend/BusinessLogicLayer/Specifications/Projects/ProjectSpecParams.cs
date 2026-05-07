using BusinessLogicLayer.Common;
using DataAccessLayer.Enums;

namespace BusinessLogicLayer.Specifications.Projects;

public class ProjectSpecParams : PaginationParams
{
    public ProjectStatus? Status { get; set; }
    public UnitStatus? UnitStatus { get; set; }
    public string? City { get; set; }
}

