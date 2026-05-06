using BusinessLogicLayer.Dtos.ProjectModule;
using BusinessLogicLayer.Specifications.Projects;
using DataAccessLayer.Common;

namespace BusinessLogicLayer.Contracts;

public interface IProjectService
{
    Task<PaginatedResult<ProjectListDto>> GetProjectsAsync(ProjectSpecParams @params);
    Task<ProjectDetailsDto?> GetProjectByIdAsync(int id);
}
