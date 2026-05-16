using BusinessLogicLayer.Common;

namespace BusinessLogicLayer.Specifications.Projects;

public class ProjectSpecParams : PaginationParams
{
    public ProjectStatus? Status { get; set; }
    public UnitStatus? UnitStatus { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool PublicOnly { get; set; } = false;
}

