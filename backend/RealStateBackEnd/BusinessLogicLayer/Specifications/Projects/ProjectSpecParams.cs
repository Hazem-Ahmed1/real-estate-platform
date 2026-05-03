using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using BusinessLogicLayer.Common;
using DataAccessLayer.Enums;

namespace BusinessLogicLayer.Specifications.Projects;

public class ProjectSpecParams : PaginationParams
{
    public ProjectStatus? Status { get; set; }
    public string? City { get; set; }
}

