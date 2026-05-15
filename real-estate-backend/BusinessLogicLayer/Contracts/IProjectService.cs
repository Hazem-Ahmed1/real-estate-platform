namespace BusinessLogicLayer.Contracts;

public interface IProjectService
{
    // Public Endpoints
    Task<PaginatedResult<ProjectListDto>> GetProjectsAsync(ProjectSpecParams @params);
    Task<ProjectDetailsDto?> GetProjectByIdAsync(int id, bool publicOnly = false);
    
    // Admin Endpoints
    Task<ProjectDetailsDto> CreateProjectAsync(ProjectDto projectDto);
    Task<ProjectDetailsDto> UpdateProjectAsync(int id, ProjectDto projectDto);
    Task<ProjectDetailsDto> DeleteProjectAsync(int id);
    
    // Lookups for Filters
    Task<List<string>> GetAvailableCitiesAsync();
}
