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

        var building = new Building
        {
            Name = dto.Name,
            ProjectId = dto.ProjectId,
            MaxArea = dto.MaxArea,
            BuildingArea = dto.BuildingArea,
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

            // 1. Move Protection: Cannot move building if it has Sold or Rented units (Historical Data Integrity)
            var hasTransactedUnits = await unitOfWork.Repository<Unit>()
                .AnyAsync(u => u.BuildingId == id && (u.Status == UnitStatus.Sold || u.Status == UnitStatus.Rented));
            
            if (hasTransactedUnits)
                throw new BadRequestException("لا يمكن نقل المبنى لمشروع آخر لأنه يحتوي على وحدات تم بيعها أو تأجيرها.");

            // 2. Path Compatibility Check: Project Sale/Sold vs Rent/Rented
            bool targetIsSalePath = targetProject.Status == ProjectStatus.Sale || targetProject.Status == ProjectStatus.Sold;
            
            // Check if building has ANY units with incompatible status
            var hasIncompatibleUnits = await unitOfWork.Repository<Unit>().AnyAsync(u => u.BuildingId == id && 
                (targetIsSalePath 
                    ? (u.Status == UnitStatus.Rent || u.Status == UnitStatus.Rented) 
                    : (u.Status == UnitStatus.Sale || u.Status == UnitStatus.Sold)));

            if (hasIncompatibleUnits)
            {
                var targetType = targetIsSalePath ? "تمليك (Sale/Sold)" : "إيجار (Rent/Rented)";
                throw new BadRequestException($"لا يمكن نقل المبنى لهذا المشروع لأن نوع وحداته غير متوافق مع نوع المشروع المستهدف ({targetType}).");
            }
        }

        var exists = await unitOfWork.Repository<Building>()
            .AnyAsync(b => b.Name == dto.Name
                        && b.ProjectId == dto.ProjectId
                        && b.BuildingId != id);

        if (exists)
            throw new ConflictException("Building with the same name already exists in this project.");

        var oldProjectId = existing.ProjectId;
        existing.Name = dto.Name;
        existing.ProjectId = dto.ProjectId;
        existing.MaxArea = dto.MaxArea;
        existing.BuildingArea = dto.BuildingArea;
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
        var building = await unitOfWork.Repository<Building>().GetByIdAsync(id);
        if (building == null)
            throw new NotFoundExpection("Building", id);

        var hasUnits = await unitOfWork.Repository<Unit>()
            .AnyAsync(u => u.BuildingId == id);

        var projectId = building.ProjectId;

        if (hasUnits)
        {
            var units = await unitOfWork.Repository<Unit>().GetAllAsync(new UnitsByBuildingSpecification(id));
            if (units.Any(u => u.Status is UnitStatus.Sold or UnitStatus.Rented))
                throw new BadRequestException("Cannot delete a building with Sold or Rented units.");
            
            // Hard delete units too if they are just Sale/Rent
            foreach (var u in units)
            {
                // Cleanup Cloudinary for each unit
                foreach (var m in u.Media)
                {
                    if (!string.IsNullOrEmpty(m.PublicId))
                    {
                        await mediaService.DeleteImageAsync(m.PublicId);
                    }
                }
                unitOfWork.Repository<Unit>().Remove(u);
            }
        }
        
        unitOfWork.Repository<Building>().Remove(building);


        await unitOfWork.CompleteAsync();

        await RecalculateProjectAsync(projectId);

        return mapper.Map<BuildingDto>(building);
    }

    private async Task RecalculateProjectAsync(int projectId)
    {
        var projectSpec = new BusinessLogicLayer.Specifications.Projects.ProjectWithBuildingsSpecification(projectId);
        var project = await unitOfWork.Repository<Project>().GetByIdAsync(projectSpec);
        if (project == null)
            return;

        var activeBuildings = project.Buildings.ToList();

        var allUnits = activeBuildings.SelectMany(b => b.Units).ToList();
        
        var oldStatus = project.Status;
        project.Status = BusinessLogicLayer.Helpers.ProjectLogicHelpers.DeriveProjectStatus(allUnits, project.Status);

        if (oldStatus != project.Status)
            project.IsStatusChanged = true;

        
        bool isSale = project.Status == ProjectStatus.Sale || project.Status == ProjectStatus.Sold;
        
        if (isSale)
        {
            project.AvailableUnitsCount = allUnits.Count(u => u.Status == UnitStatus.Sale);
            project.TransactedUnitsCount = allUnits.Count(u => u.Status == UnitStatus.Sold);
        }
        else
        {
            project.AvailableUnitsCount = allUnits.Count(u => u.Status == UnitStatus.Rent);
            project.TransactedUnitsCount = allUnits.Count(u => u.Status == UnitStatus.Rented);
        }

        // Recalculate Building FloorCounts and Project Area/Status
        foreach (var b in activeBuildings)
        {
            b.FloorCount = b.Units.Any() ? b.Units.Max(u => u.Floor) : 0;
            unitOfWork.Repository<Building>().Update(b);
        }

        // TotalBuildingArea = Σ (BuildingArea_i) + Σ (Max unit area in Building_i)
        project.TotalBuildingArea = activeBuildings.Sum(b => b.BuildingArea + (b.Units.Any() ? b.Units.Max(u => u.Area) : 0));

        unitOfWork.Repository<Project>().Update(project);
        await unitOfWork.CompleteAsync();

    }

}
