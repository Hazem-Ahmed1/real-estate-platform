namespace BusinessLogicLayer.Contracts;

public interface IProjectService
{
    // Public Endpoints
    Task<PaginatedResult<ProjectListDto>> GetProjectsAsync(ProjectSpecParams @params);
    Task<ProjectDetailsDto?> GetProjectByIdAsync(int id, bool publicOnly = false);
    
    // Admin Endpoints
    Task<ProjectDetailsDto> CreateProjectAsync(ProjectCreateDto projectDto);
    Task<ProjectDetailsDto> UpdateProjectAsync(int id, ProjectUpdateDto projectDto);
    Task<ProjectDetailsDto> DeleteProjectAsync(int id);
}
