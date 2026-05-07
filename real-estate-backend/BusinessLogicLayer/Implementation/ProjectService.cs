using AutoMapper;
using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.ProjectModule;
using BusinessLogicLayer.Exceptions;
using BusinessLogicLayer.Specifications.Projects;
using DataAccessLayer.Contracts;
using DataAccessLayer.Common;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Enums;

namespace BusinessLogicLayer.Implementation;

public class ProjectService(IUnitOfWork unitOfWork, IMapper mapper, IMediaService mediaService) : IProjectService
{
    public async Task<PaginatedResult<ProjectListDto>> GetProjectsAsync(ProjectSpecParams @params)
    {
        var spec = new ProjectWithBuildingsSpecification(@params);
        var countSpec = new ProjectWithBuildingsSpecification(@params, true);

        var totalItems = await unitOfWork.Repository<Project>().CountAsync(countSpec);
        var projects = await unitOfWork.Repository<Project>().GetAllAsync(spec);

        // Ensure project status is derived from contained units and persisted if changed
        var updated = false;
        foreach (var project in projects)
        {
            var allUnits = project.Buildings.SelectMany(b => b.Units).ToList();
            var derived = DeriveProjectStatus(allUnits);
            var derivedTotalArea = DeriveProjectTotalArea(allUnits);
            var currentTotalArea = project.TotalArea ?? 0d;

            if (project.Status != derived || Math.Abs(currentTotalArea - derivedTotalArea) > 0.000001d)
            {
                project.Status = derived;
                project.TotalArea = derivedTotalArea;
                unitOfWork.Repository<Project>().Update(project);
                updated = true;
            }
        }

        if (updated)
            await unitOfWork.CompleteAsync();

        var data = mapper.Map<IReadOnlyList<ProjectListDto>>(projects);

        return new PaginatedResult<ProjectListDto>(@params.Page, @params.PageSize, totalItems, data);
    }

    public async Task<GetProjectDto?> GetProjectByIdAsync(int id)
    {
        var spec = new ProjectWithBuildingsSpecification(id);
        var project = await unitOfWork.Repository<Project>().GetByIdAsync(spec);

        if (project == null)
            return null;

        var allUnits = project.Buildings.SelectMany(b => b.Units).ToList();
        var derived = DeriveProjectStatus(allUnits);
        var derivedTotalArea = DeriveProjectTotalArea(allUnits);
        var currentTotalArea = project.TotalArea ?? 0d;

        if (project.Status != derived || Math.Abs(currentTotalArea - derivedTotalArea) > 0.000001d)
        {
            project.Status = derived;
            project.TotalArea = derivedTotalArea;
            unitOfWork.Repository<Project>().Update(project);
            await unitOfWork.CompleteAsync();
        }

        return mapper.Map<GetProjectDto>(project);
    }

    public async Task<GetProjectDto> CreateProjectAsync(CreateProjectDto projectDto)
    {
        // Conflict check
        var exists = await unitOfWork.Repository<Project>()
            .AnyAsync(p => p.Name == projectDto.Name && !p.IsDeleted);

        if (exists)
            throw new ConflictException("Project with the same name already exists.");

        var project = mapper.Map<Project>(projectDto);
        project.Status = ProjectStatus.Mixed;
        await unitOfWork.Repository<Project>().AddAsync(project);
        await unitOfWork.CompleteAsync();

        var created = await GetProjectByIdAsync(project.ProjectId);
        if (created == null)
            throw new NotFoundExpection("Project", project.ProjectId);

        return created;
    }

    public async Task<GetProjectDto> UpdateProjectAsync(int id, UpdateProjectDto projectDto)
    {
        var existing = await unitOfWork.Repository<Project>().GetByIdAsync(id);

        if (existing == null)
            throw new NotFoundExpection("Project", id);

        var exists = await unitOfWork.Repository<Project>()
            .AnyAsync(p => p.Name == projectDto.Name && p.ProjectId != id && !p.IsDeleted);

        if (exists)
            throw new ConflictException("Project with the same name already exists.");

        mapper.Map(projectDto, existing);
        unitOfWork.Repository<Project>().Update(existing);
        await unitOfWork.CompleteAsync();

        var updatedProject = await GetProjectByIdAsync(id);
        if (updatedProject == null)
            throw new NotFoundExpection("Project", id);

        return updatedProject;
    }

    public async Task<GetProjectDto> DeleteProjectAsync(int id)
    {
        var project = await unitOfWork.Repository<Project>().GetByIdAsync(id);

        if (project == null)
            throw new NotFoundExpection("Project", id);

        var hasBuildings = await unitOfWork.Repository<Building>()
            .AnyAsync(b => b.ProjectId == id);

        if (hasBuildings)
        {
            // Soft Delete
            project.IsDeleted = true;
            unitOfWork.Repository<Project>().Update(project);
        }
        else
        {
            // Hard Delete
            unitOfWork.Repository<Project>().Remove(project);
        }

        await unitOfWork.CompleteAsync();

        if (hasBuildings)
        {
            var deletedProject = await GetProjectByIdAsync(id);
            if (deletedProject == null)
                throw new NotFoundExpection("Project", id);
            return deletedProject;
        }

        return new GetProjectDto
        {
            ProjectId = project.ProjectId,
            Name = project.Name,
            Status = project.Status,
            IsDeleted = true,
            Address = project.Address,
            Latitude = project.Latitude,
            Longitude = project.Longitude
        };
    }

    public async Task<IReadOnlyList<BuildingDto>> GetProjectBuildingsAsync(int projectId)
    {
        var exists = await unitOfWork.Repository<Project>()
            .AnyAsync(p => p.ProjectId == projectId);

        if (!exists)
            throw new NotFoundExpection("Project", projectId);

        var buildings = await unitOfWork.Repository<Building>().GetAllAsync();
        var projectBuildings = buildings.Where(b => b.ProjectId == projectId);

        return mapper.Map<IReadOnlyList<BuildingDto>>(projectBuildings);
    }

    public async Task<BuildingDto> CreateProjectBuildingAsync(int projectId, BuildingUpsertDto dto)
    {
        var project = await unitOfWork.Repository<Project>().GetByIdAsync(projectId);

        if (project == null)
            throw new NotFoundExpection("Project", projectId);

        var exists = await unitOfWork.Repository<Building>()
            .AnyAsync(b => b.Name == dto.Name
                        && b.ProjectId == projectId
                        && !b.IsDeleted);

        if (exists)
            throw new ConflictException("Building with the same name already exists in this project.");

        var building = new Building
        {
            Name = dto.Name,
            Floors = dto.Floors,
            ProjectId = projectId
        };

        await unitOfWork.Repository<Building>().AddAsync(building);
        await unitOfWork.CompleteAsync();

        return mapper.Map<BuildingDto>(building);
    }

    public async Task<ProjectMediaDto> AddProjectMediaAsync(int projectId, Microsoft.AspNetCore.Http.IFormFile file, MediaType type, bool isThumbnail = false)
    {
        var project = await unitOfWork.Repository<Project>().GetByIdAsync(projectId);

        if (project == null)
            throw new NotFoundExpection("Project", projectId);

        var url = await mediaService.UploadImageAsync(file);

        var media = new ProjectMedia
        {
            ProjectId = projectId,
            MediaUrl = url.Url,
            Type = type,
            IsThumbnail = isThumbnail
        };

        await unitOfWork.Repository<ProjectMedia>().AddAsync(media);
        await unitOfWork.CompleteAsync();

        return new ProjectMediaDto
        {
            MediaId = media.MediaId,
            ProjectId = media.ProjectId,
            Type = media.Type.ToString(),
            MediaUrl = media.MediaUrl,
            ThumbnailUrl = media.ThumbnailUrl,
            IsThumbnail = media.IsThumbnail
        };
    }

    public async Task<ProjectMediaDto> DeleteProjectMediaAsync(int projectId, int mediaId)
    {
        var media = await unitOfWork.Repository<ProjectMedia>().GetByIdAsync(mediaId);
        if (media == null || media.ProjectId != projectId)
            throw new NotFoundExpection("ProjectMedia", mediaId);

        var deletedMediaDto = new ProjectMediaDto
        {
            MediaId = media.MediaId,
            ProjectId = media.ProjectId,
            Type = media.Type.ToString(),
            MediaUrl = media.MediaUrl,
            ThumbnailUrl = media.ThumbnailUrl,
            IsThumbnail = media.IsThumbnail
        };

        unitOfWork.Repository<ProjectMedia>().Remove(media);
        await unitOfWork.CompleteAsync();
        return deletedMediaDto;
    }



    public async Task<List<string>> GetAvailableCitiesAsync()
    {
        var projects = await unitOfWork.Repository<Project>().GetAllAsync();
        return projects
            .Select(p => p.City)
            .Where(c => c != null)
            .Distinct()
            .ToList()!;
    }

    public async Task<List<string>> GetSortOptionsAsync()
    {
        return new List<string> { "dateAsc", "dateDesc", "priceAsc", "priceDesc", "nameAsc", "nameDesc" };
    }

    private static ProjectStatus DeriveProjectStatus(IEnumerable<DataAccessLayer.Entities.UnitModule.Unit> units)
    {
        var statuses = units.Select(u => u.Status).ToList();
        if (statuses.Count == 0)
            return ProjectStatus.Mixed;

        if (statuses.All(s => s == UnitStatus.ForSale))
            return ProjectStatus.AllForSale;

        if (statuses.All(s => s == UnitStatus.ForRent))
            return ProjectStatus.AllForRent;

        if (statuses.All(s => s == UnitStatus.Sold))
            return ProjectStatus.AllForSoldOut;

        if (statuses.All(s => s == UnitStatus.Rented))
            return ProjectStatus.AllForRented;

        return ProjectStatus.Mixed;
    }

    private static double DeriveProjectTotalArea(IEnumerable<DataAccessLayer.Entities.UnitModule.Unit> units)
    {
        // Total area is the sum of unit areas; null areas are treated as 0
        return units.Sum(u => u.Area ?? 0d);
    }
}
