using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.BuildingModule;
using BusinessLogicLayer.Specifications;
using BusinessLogicLayer.Specifications.Buildings;
using AutoMapper;
using DataAccessLayer.Contracts;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Enums;
using DataAccessLayer.Common;

namespace BusinessLogicLayer.Implementation;

public class BuildingService(IUnitOfWork unitOfWork, IMapper mapper, IMediaService mediaService) : IBuildingService
{
    public async Task<IReadOnlyList<BuildingDto>> GetAllBuildingsAsync(int? projectId = null)
    {
        ISpecification<Building> spec;
        if (projectId.HasValue)
            spec = new BuildingsByProjectSpecification(projectId.Value);
        else
            spec = new BuildingWithProjectSpecification();

        var buildings = await unitOfWork.Repository<Building>().GetAllAsync(spec);
        return mapper.Map<IReadOnlyList<BuildingDto>>(buildings);
    }

    public async Task<IReadOnlyList<BuildingDto>> GetBuildingsByProjectIdAsync(int projectId)
    {
        var spec = new BuildingsByProjectSpecification(projectId);
        var buildings = await unitOfWork.Repository<Building>().GetAllAsync(spec);
        return mapper.Map<IReadOnlyList<BuildingDto>>(buildings);
    }

    public async Task<BuildingDto> GetBuildingByIdAsync(int id)
    {
        var building = await unitOfWork.Repository<Building>().GetByIdAsync(id);
        if (building == null)
            throw new NotFoundExpection("Building", id);

        return mapper.Map<BuildingDto>(building);
    }

    public async Task<BuildingDto> CreateBuildingAsync(BuildingUpsertDto dto)
    {
        var project = await unitOfWork.Repository<Project>()
            .GetByIdAsync(dto.ProjectId);

        if (project == null)
            throw new NotFoundExpection("Project", dto.ProjectId);


        var exists = await unitOfWork.Repository<Building>()
            .AnyAsync(b => b.Name == dto.Name
                        && b.ProjectId == dto.ProjectId);

        if (exists)
            throw new ConflictException("Building with the same name already exists in this project.");

        // Building type compatibility: building type must match project's nature
        bool projectIsSalePath = project.Status == ProjectStatus.Sale || project.Status == ProjectStatus.Sold;
        if (dto.Type == BuildingType.Sale && !projectIsSalePath)
            throw new BadRequestException("Cannot add a Sale building to a Rent project.");
        if (dto.Type == BuildingType.Rent && projectIsSalePath)
            throw new BadRequestException("Cannot add a Rent building to a Sale project.");

        // Validate total building footprint does not exceed project's BuildUpArea
        var existingBuildingsArea = await unitOfWork.Repository<Building>()
            .GetAllAsync(new BuildingsByProjectSpecification(dto.ProjectId));
        var currentTotalBuildingArea = existingBuildingsArea.Sum(b => b.BuildingArea);

        if (currentTotalBuildingArea + dto.BuildingArea > project.BuildUpArea)
            throw new BadRequestException(
                $"Adding this building would exceed the project's build-up area. " +
                $"Available: {project.BuildUpArea - currentTotalBuildingArea} m², Requested: {dto.BuildingArea} m².");

        // Ensure MaxArea (max area per unit / floor) does not exceed the building footprint
        if (dto.MaxArea > dto.BuildingArea)
            throw new BadRequestException("MaxArea must be less than or equal to BuildingArea.");

        var building = new Building
        {
            Name = dto.Name,
            ProjectId = dto.ProjectId,
            MaxArea = dto.MaxArea,
            BuildingArea = dto.BuildingArea,
            Type = dto.Type,
            FloorCount = dto.FloorCount
        };


        await unitOfWork.Repository<Building>().AddAsync(building);
        await unitOfWork.CompleteAsync();

        await RecalculateProjectAsync(dto.ProjectId);

        return mapper.Map<BuildingDto>(building);
    }

    public async Task<BuildingDto> UpdateBuildingAsync(int id, BuildingUpsertDto dto)
    {
        var existing = await unitOfWork.Repository<Building>().GetByIdAsync(id);

        if (existing == null)
            throw new NotFoundExpection("Building", id);

        if (dto.ProjectId != existing.ProjectId)
        {
            var targetProject = await unitOfWork.Repository<Project>()
                .GetByIdAsync(dto.ProjectId);

            if (targetProject == null)
                throw new NotFoundExpection("Project", dto.ProjectId);

            // Building type compatibility: building type must match target project's nature
            bool targetIsSalePath = targetProject.Status == ProjectStatus.Sale || targetProject.Status == ProjectStatus.Sold;
            if (existing.Type == BuildingType.Sale && !targetIsSalePath)
                throw new BadRequestException("Cannot move a Sale building into a Rent project.");
            if (existing.Type == BuildingType.Rent && targetIsSalePath)
                throw new BadRequestException("Cannot move a Rent building into a Sale project.");
        }

        var exists = await unitOfWork.Repository<Building>()
            .AnyAsync(b => b.Name == dto.Name
                        && b.ProjectId == dto.ProjectId
                        && b.BuildingId != id);

        if (exists)
            throw new ConflictException("Building with the same name already exists in this project.");

        // Validate total building footprint does not exceed project's BuildUpArea (excluding current building)
        var targetProjectToValidate = await unitOfWork.Repository<Project>().GetByIdAsync(dto.ProjectId);
        if (targetProjectToValidate == null)
            throw new NotFoundExpection("Project", dto.ProjectId);

        var allBuildingsInProject = await unitOfWork.Repository<Building>()
            .GetAllAsync(new BuildingsByProjectSpecification(dto.ProjectId));
        var currentTotalExcludingSelf = allBuildingsInProject
            .Where(b => b.BuildingId != id)
            .Sum(b => b.BuildingArea);

        if (currentTotalExcludingSelf + dto.BuildingArea > targetProjectToValidate.BuildUpArea)
            throw new BadRequestException(
                $"Updating this building's area would exceed the project's build-up area. " +
                $"Available: {targetProjectToValidate.BuildUpArea - currentTotalExcludingSelf} m², Requested: {dto.BuildingArea} m².");

        // Ensure MaxArea (max area per unit / floor) does not exceed the building footprint
        if (dto.MaxArea > dto.BuildingArea)
            throw new BadRequestException("MaxArea must be less than or equal to BuildingArea.");

        var oldProjectId = existing.ProjectId;
        var unitsInBuilding = await unitOfWork.Repository<Unit>()
            .GetAllAsync(new UnitsByBuildingSpecification(id));

        if (existing.Type != dto.Type && unitsInBuilding.Any())
        {
            throw new BadRequestException("Cannot change the Building Type because it already contains units. You must delete the units first.");
        }

        existing.Name = dto.Name;
        existing.ProjectId = dto.ProjectId;
        existing.MaxArea = dto.MaxArea;
        existing.BuildingArea = dto.BuildingArea;
        existing.Type = dto.Type;
        existing.FloorCount = dto.FloorCount;


        unitOfWork.Repository<Building>().Update(existing);
        await unitOfWork.CompleteAsync();


        if (oldProjectId != dto.ProjectId)
        {
            await RecalculateProjectAsync(oldProjectId);
            await RecalculateProjectAsync(dto.ProjectId);
        }
        else
        {
            await RecalculateProjectAsync(oldProjectId);
        }


        return mapper.Map<BuildingDto>(existing);
    }

    public async Task<BuildingDto> DeleteBuildingAsync(int id)
    {
        // Load building with all its units and their media for cascade delete
        var spec = new BuildingsByProjectSpecification(0); // we'll get by id below
        var building = await unitOfWork.Repository<Building>().GetByIdAsync(id);
        if (building == null)
            throw new NotFoundExpection("Building", id);

        var projectId = building.ProjectId;
        var deletedDto = mapper.Map<BuildingDto>(building);

        // Cascade delete: remove all units and their Cloudinary media first
        var units = await unitOfWork.Repository<Unit>()
            .GetAllAsync(new UnitsByBuildingSpecification(id));

        var cloudinaryImagesToDelete = new List<string>();
        var cloudinaryVideosToDelete = new List<string>();

        foreach (var unit in units)
        {
            foreach (var m in unit.Media.ToList())
            {
                if (!string.IsNullOrEmpty(m.PublicId))
                {
                    if (m.Type == MediaType.Video) cloudinaryVideosToDelete.Add(m.PublicId);
                    else cloudinaryImagesToDelete.Add(m.PublicId);
                }
                unitOfWork.Repository<UnitMedia>().Remove(m);
            }
            unitOfWork.Repository<Unit>().Remove(unit);
        }

        unitOfWork.Repository<Building>().Remove(building);
        await unitOfWork.CompleteAsync();

        // Cleanup Cloudinary after successful DB commit
        foreach (var pid in cloudinaryImagesToDelete) await mediaService.DeleteImageAsync(pid);
        foreach (var pid in cloudinaryVideosToDelete) await mediaService.DeleteVideoAsync(pid);

        await RecalculateProjectAsync(projectId);

        return deletedDto;
    }

    private async Task RecalculateProjectAsync(int projectId)
    {
        var projectSpec = new BusinessLogicLayer.Specifications.Projects.ProjectWithBuildingsSpecification(projectId);
        var project = await unitOfWork.Repository<Project>().GetByIdAsync(projectSpec);
        if (project == null) return;

        var allUnits = project.Buildings.SelectMany(b => b.Units).ToList();

        // 1. Update each building's derived status
        foreach (var building in project.Buildings)
        {
            building.Status = ProjectLogicHelpers.DeriveBuildingStatus(building.Type, building.Units);
            // Only derive floor count from units when units exist. Preserve explicit floor count otherwise.
            if (building.Units.Any())
            {
                building.FloorCount = building.Units.Max(u => u.Floor);
            }
            unitOfWork.Repository<Building>().Update(building);
        }

        // 2. Update project status
        project.Status = ProjectLogicHelpers.DeriveProjectStatus(allUnits, project.Status);

        project.AvailableUnitsCount  = allUnits.Count(u => u.Status == UnitStatus.Sale   || u.Status == UnitStatus.Rent);
        project.TransactedUnitsCount = allUnits.Count(u => u.Status == UnitStatus.Sold   || u.Status == UnitStatus.Rented);
        project.TotalBuildingArea    = project.Buildings.Sum(b => b.BuildingArea);

        unitOfWork.Repository<Project>().Update(project);
        await unitOfWork.CompleteAsync();
    }

}
