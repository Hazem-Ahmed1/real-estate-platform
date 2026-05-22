using BusinessLogicLayer.Common;

using DataAccessLayer.Enums;

namespace BusinessLogicLayer.Specifications.Projects;

public class ProjectSpecParams : PaginationParams
{
    public ProjectStatus? Status { get; set; }
    public UnitStatus? UnitStatus { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? City { get; set; }
    public UnitType? Type { get; set; }
    public int? Rooms { get; set; }
}

