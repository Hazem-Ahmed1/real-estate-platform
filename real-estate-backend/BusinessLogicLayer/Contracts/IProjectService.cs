using BusinessLogicLayer.Dtos.ProjectModule;
using BusinessLogicLayer.Specifications.Projects;
using DataAccessLayer.Common;

namespace BusinessLogicLayer.Contracts;

public interface IProjectService
{
    // Public Endpoints
    Task<PaginatedResult<ProjectListDto>> GetProjectsAsync(ProjectSpecParams @params);
    Task<GetProjectDto?> GetProjectByIdAsync(int id);
    
    // Admin Endpoints
    Task<GetProjectDto> CreateProjectAsync(CreateProjectDto projectDto);
    Task<GetProjectDto> UpdateProjectAsync(int id, UpdateProjectDto projectDto);
    Task<GetProjectDto> DeleteProjectAsync(int id);
    
    // Project Buildings
    Task<IReadOnlyList<BuildingDto>> GetProjectBuildingsAsync(int projectId);
    Task<BuildingDto> CreateProjectBuildingAsync(int projectId, BuildingUpsertDto dto);

    // Project Media
    Task<ProjectMediaDto> AddProjectMediaAsync(int projectId, Microsoft.AspNetCore.Http.IFormFile file, DataAccessLayer.Enums.MediaType type, bool isThumbnail = false);
    Task<ProjectMediaDto> DeleteProjectMediaAsync(int projectId, int mediaId);


    // Lookups for Filters
    Task<List<string>> GetAvailableCitiesAsync();
    Task<List<string>> GetSortOptionsAsync();
}

