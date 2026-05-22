using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace BusinessLogicLayer.Implementation;

public class UnitService(IUnitOfWork unitOfWork, IMapper mapper, IMediaService mediaService, IHttpClientFactory httpClientFactory, IConfiguration configuration) : IUnitService
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient();
    private readonly string overpassUrl = configuration["Overpass:BaseUrl"] ?? "https://overpass-api.de/api/interpreter";
    private readonly int overpassRadiusMeters = int.TryParse(configuration["Overpass:RadiusMeters"], out var radius) ? radius : 2000;
    private readonly int overpassMaxResults = int.TryParse(configuration["Overpass:MaxResults"], out var maxResults) ? maxResults : 5;
    private readonly string overpassLanguage = configuration["Overpass:Language"] ?? "ar";

    public async Task<PaginatedResult<UnitListDto>> GetUnitsAsync(UnitSpecParams @params)
    {
        var spec = new UnitWithDetailsSpecification(@params);
        var countSpec = new UnitWithFiltersForCountSpecification(@params);

        var total = await unitOfWork.Repository<Unit>().CountAsync(countSpec);
        var units = await unitOfWork.Repository<Unit>().GetAllAsync(spec);

        var data = mapper.Map<IReadOnlyList<UnitListDto>>(units);
        return new PaginatedResult<UnitListDto>(@params.Page, @params.PageSize, total, data);
    }

    public async Task<PaginatedResult<UnitListDto>> GetAdminUnitsAsync(UnitSpecParams @params)
    {
        @params.IncludeAllStatuses = true;
        return await GetUnitsAsync(@params);
    }

    public async Task<GetUnitDto?> GetUnitByIdAsync(int id)
    {
        var spec = new UnitWithDetailsSpecification(id);
        var unit = await unitOfWork.Repository<Unit>().GetByIdAsync(spec);

        return unit == null ? null : mapper.Map<GetUnitDto>(unit);
    }

    public async Task<GetUnitDto> CreateUnitAsync(UnitCreateDto unitDto)
    {
        ValidateMediaFiles(unitDto.ThumbnailImage, unitDto.Images, unitDto.Designs, unitDto.Panorama360, unitDto.VideoFile);

        var building = await unitOfWork.Repository<Building>()
            .GetByIdAsync(unitDto.BuildingId);

        if (building == null)
            throw new NotFoundExpection("Building", unitDto.BuildingId);


        var project = await unitOfWork.Repository<Project>().GetByIdAsync(building.ProjectId);
        if (project == null)
            throw new NotFoundExpection("Project", building.ProjectId);


        if (unitDto.Status == UnitStatus.Sold || unitDto.Status == UnitStatus.Rented)
            throw new BadRequestException("Units cannot be created with 'Sold' or 'Rented' status.");

        // Building Type Compatibility Check
        if (building.Type == BuildingType.Sale && (unitDto.Status == UnitStatus.Rent || unitDto.Status == UnitStatus.Rented))
            throw new BadRequestException("This building is for Sale only. Cannot add Rent units.");
        
        if (building.Type == BuildingType.Rent && (unitDto.Status == UnitStatus.Sale || unitDto.Status == UnitStatus.Sold))
            throw new BadRequestException("This building is for Rent only. Cannot add Sale units.");

        bool isSaleProject = project.Status == ProjectStatus.Sale || project.Status == ProjectStatus.Sold;

        // Building Type Exclusivity Rule
        bool incomingIsVilla = unitDto.Type == UnitType.Villa;
        var existingBuildingUnits = await unitOfWork.Repository<Unit>()
            .GetAllAsync(new UnitsByBuildingSpecification(unitDto.BuildingId));
        bool buildingHasUnits = existingBuildingUnits.Any();

        if (buildingHasUnits)
        {
            bool buildingHasVilla = existingBuildingUnits.Any(u => u.Type == UnitType.Villa);
            bool buildingHasNonVilla = existingBuildingUnits.Any(u => u.Type != UnitType.Villa);

            if (incomingIsVilla && buildingHasNonVilla)
                throw new BadRequestException(
                    "Cannot add a Villa to this building. The building already contains non-Villa units (Apartment/Duplex/Office).");

            if (!incomingIsVilla && buildingHasVilla)
                throw new BadRequestException(
                    "Cannot add this unit to a Villa building. Villa buildings can only contain Villa units.");
        }

        if (unitDto.Area <= 0)
            throw new BadRequestException("Unit area must be greater than 0.");

        if (unitDto.Area > building.MaxArea)
            throw new BadRequestException($"Unit area ({unitDto.Area}) exceeds the building's designated maximum area ({building.MaxArea}).");

        // Floor validation: if building has a configured floor count, ensure unit floor doesn't exceed it
        if (building.FloorCount.HasValue && unitDto.Floor > building.FloorCount.Value)
            throw new BadRequestException($"Unit floor ({unitDto.Floor}) exceeds building's floor count ({building.FloorCount.Value}).");

        // Rooms/Salons/Bathrooms/StreetCount validation (additional runtime checks with friendly messages)
        if (unitDto.Rooms < 1 || unitDto.Rooms > 50)
            throw new BadRequestException("عدد الغرف غير صالح. يجب أن يكون بين 1 و 50.");
        if (unitDto.Salons < 0 || unitDto.Salons > 50)
            throw new BadRequestException("عدد الصالونات غير صالح. يجب أن يكون بين 0 و 50.");
        if (unitDto.Bathrooms < 1 || unitDto.Bathrooms > 50)
            throw new BadRequestException("عدد الحمامات غير صالح. يجب أن يكون بين 1 و 50.");
        if (unitDto.StreetCount < 1 || unitDto.StreetCount > 4)
            throw new BadRequestException("عدد الشوارع يجب أن يكون بين 1 و 4.");

        // Derive city/region/address from parent project to avoid duplicates and enforce consistency
        unitDto.City = project.City;
        unitDto.Region = project.Region;
        // If frontend provided an address (from unit map pick) keep it, otherwise fall back to project address
        if (string.IsNullOrWhiteSpace(unitDto.Address))
        {
            unitDto.Address = project.Address;
        }


        var exists = await unitOfWork.Repository<Unit>()
            .AnyAsync(u => u.Name == unitDto.Name && u.BuildingId == unitDto.BuildingId);

        if (exists)
            throw new ConflictException("Unit with the same name already exists in this building.");

        var unit = mapper.Map<Unit>(unitDto);

        if (unitDto.FeatureIds.Any())
        {
            var features = await unitOfWork.Repository<Feature>().GetAllAsync();
            foreach (var featureId in unitDto.FeatureIds)
            {
                var feature = features.FirstOrDefault(f => f.FeatureId == featureId);
                if (feature == null)
                    throw new NotFoundExpection("Feature", featureId);

                if (!feature.IsActive)
                    throw new BadRequestException($"الميزة '{feature.Name}' معطلة ولا يمكن إضافتها حالياً.");

                unit.UnitFeatures.Add(new UnitFeature { FeatureId = featureId });
            }
        }

        if (unitDto.InsuranceIds.Any())
        {
            var insurances = await unitOfWork.Repository<Insurance>().GetAllAsync();
            foreach (var insuranceId in unitDto.InsuranceIds)
            {
                var insurance = insurances.FirstOrDefault(i => i.InsuranceId == insuranceId);
                if (insurance == null)
                    throw new NotFoundExpection("Insurance", insuranceId);

                if (!insurance.IsActive)
                    throw new BadRequestException($"التأمين '{insurance.Name}' معطل ولا يمكن إضافته حالياً.");

                unit.UnitInsurance.Add(new UnitInsurance { InsuranceId = insuranceId });
            }
        }

        var nearbyFacilities = unitDto.NearbyFacilities.Any()
            ? DeduplicateNearbyFacilities(unitDto.NearbyFacilities.Select(dto => mapper.Map<NearbyFacility>(dto)))
            : DeduplicateNearbyFacilities(await FetchNearbyFacilitiesAsync(unitDto.Latitude, unitDto.Longitude));

        foreach (var facility in nearbyFacilities)
        {
            unit.NearbyFacilities.Add(facility);
        }

        var newlyUploadedImageIds = new List<string>();
        var newlyUploadedVideoIds = new List<string>();

        try
        {
            if (unitDto.ThumbnailImage != null)
            {
                var res = await mediaService.UploadImageAsync(unitDto.ThumbnailImage);
                newlyUploadedImageIds.Add(res.PublicId);
                unit.Media.Add(new UnitMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Image, IsThumbnail = true });
            }

            if (unitDto.Images != null && unitDto.Images.Any())
            {
                foreach (var img in unitDto.Images)
                {
                    var res = await mediaService.UploadImageAsync(img);
                    newlyUploadedImageIds.Add(res.PublicId);
                    unit.Media.Add(new UnitMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Image, IsThumbnail = false });
                }
            }

            if (unitDto.Designs != null && unitDto.Designs.Any())
            {
                foreach (var img in unitDto.Designs)
                {
                    var res = await mediaService.UploadImageAsync(img);
                    newlyUploadedImageIds.Add(res.PublicId);
                    unit.Media.Add(new UnitMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Design, IsThumbnail = false });
                }
            }

            if (unitDto.Panorama360 != null)
            {
                var res = await mediaService.UploadImageAsync(unitDto.Panorama360);
                newlyUploadedImageIds.Add(res.PublicId);
                unit.Media.Add(new UnitMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Panorama360, IsThumbnail = false });
            }

            if (unitDto.VideoFile != null)
            {
                var res = await mediaService.UploadVideoAsync(unitDto.VideoFile);
                newlyUploadedVideoIds.Add(res.PublicId);
                unit.Media.Add(new UnitMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Video, IsThumbnail = false });
            }


            await unitOfWork.Repository<Unit>().AddAsync(unit);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception)
        {
            foreach (var pid in newlyUploadedImageIds) await mediaService.DeleteImageAsync(pid);
            foreach (var pid in newlyUploadedVideoIds) await mediaService.DeleteVideoAsync(pid);
            throw;
        }

        await RecalculateProjectStatusAsync(building.ProjectId);

        var created = await GetUnitByIdAsync(unit.UnitId);
        if (created == null)
            throw new NotFoundExpection("Unit", unit.UnitId);

        return created;
    }

    public async Task<GetUnitDto> UpdateUnitAsync(int id, UnitUpdateDto unitDto)
    {
        ValidateMediaFiles(unitDto.ThumbnailImage, unitDto.Images, unitDto.Designs, unitDto.Panorama360, unitDto.VideoFile);

        var spec = new UnitWithDetailsSpecification(id);
        var existing = await unitOfWork.Repository<Unit>().GetByIdAsync(spec);

        if (existing == null)
            throw new NotFoundExpection("Unit", id);

        var oldBuildingId = existing.BuildingId;
        var oldProjectId = await GetProjectIdByBuildingIdAsync(oldBuildingId);

        var targetBuilding = await unitOfWork.Repository<Building>().GetByIdAsync(unitDto.BuildingId);
        if (targetBuilding == null)
            throw new NotFoundExpection("Building", unitDto.BuildingId);

        // Building Type Compatibility Check
        if (targetBuilding.Type == BuildingType.Sale && (unitDto.Status == UnitStatus.Rent || unitDto.Status == UnitStatus.Rented))
            throw new BadRequestException("Target building is for Sale only.");
        
        if (targetBuilding.Type == BuildingType.Rent && (unitDto.Status == UnitStatus.Sale || unitDto.Status == UnitStatus.Sold))
            throw new BadRequestException("Target building is for Rent only.");

        var project = await unitOfWork.Repository<Project>().GetByIdAsync(targetBuilding.ProjectId);
        if (project == null)
            throw new NotFoundExpection("Project", targetBuilding.ProjectId);


        bool isSaleProject = project.Status == ProjectStatus.Sale || project.Status == ProjectStatus.Sold;

        if (isSaleProject && (unitDto.Status == UnitStatus.Rent || unitDto.Status == UnitStatus.Rented))
            throw new BadRequestException("Unit status must be Sale or Sold because the project is Sale-type.");

        if (!isSaleProject && (unitDto.Status == UnitStatus.Sale || unitDto.Status == UnitStatus.Sold))
            throw new BadRequestException("Unit status must be Rent or Rented because the project is Rent-type.");

        // Building Type Exclusivity Rule
        bool incomingIsVillaUpdate = unitDto.Type == UnitType.Villa;
        var targetBuildingUnits = await unitOfWork.Repository<Unit>()
            .GetAllAsync(new UnitsByBuildingSpecification(unitDto.BuildingId));
            
        var otherUnitsInBuilding = targetBuildingUnits
            .Where(u => u.UnitId != id)
            .ToList();

        if (otherUnitsInBuilding.Any())
        {
            bool buildingHasVilla = otherUnitsInBuilding.Any(u => u.Type == UnitType.Villa);
            bool buildingHasNonVilla = otherUnitsInBuilding.Any(u => u.Type != UnitType.Villa);

            if (incomingIsVillaUpdate && buildingHasNonVilla)
                throw new BadRequestException(
                    "Cannot change this unit to Villa type. The building already contains non-Villa units.");

            if (!incomingIsVillaUpdate && buildingHasVilla)
                throw new BadRequestException(
                    "Cannot add a non-Villa unit to a Villa building.");
        }

        if (unitDto.Area <= 0)
            throw new BadRequestException("Unit area data is invalid.");

        if (unitDto.Area > targetBuilding.MaxArea)
            throw new BadRequestException("Unit area exceeds the maximum allowed area for a floor in this building.");

        // Validate floor does not exceed building's configured floor count
        if (targetBuilding.FloorCount.HasValue && unitDto.Floor > targetBuilding.FloorCount.Value)
            throw new BadRequestException($"Unit floor ({unitDto.Floor}) exceeds building's floor count ({targetBuilding.FloorCount.Value}).");

        // Rooms/Salons/Bathrooms/StreetCount checks on update as well
        if (unitDto.Rooms < 1 || unitDto.Rooms > 50)
            throw new BadRequestException("عدد الغرف غير صالح. يجب أن يكون بين 1 و 50.");
        if (unitDto.Salons < 0 || unitDto.Salons > 50)
            throw new BadRequestException("عدد الصالونات غير صالح. يجب أن يكون بين 0 و 50.");
        if (unitDto.Bathrooms < 1 || unitDto.Bathrooms > 50)
            throw new BadRequestException("عدد الحمامات غير صالح. يجب أن يكون بين 1 و 50.");
        if (unitDto.StreetCount < 1 || unitDto.StreetCount > 4)
            throw new BadRequestException("عدد الشوارع يجب أن يكون بين 1 و 4.");

        // Ensure unit's city/region derive from target project's values
        var targetProject = await unitOfWork.Repository<Project>().GetByIdAsync(targetBuilding.ProjectId);
        if (targetProject == null)
            throw new NotFoundExpection("Project", targetBuilding.ProjectId);

        unitDto.City = targetProject.City;
        unitDto.Region = targetProject.Region;
        if (string.IsNullOrWhiteSpace(unitDto.Address))
        {
            unitDto.Address = targetProject.Address;
        }

        if (existing.Status is UnitStatus.Sold or UnitStatus.Rented && unitDto.BuildingId != oldBuildingId)
            throw new BadRequestException("This unit cannot be moved because it has already been transacted (Sold/Rented).");


        var nameConflict = await unitOfWork.Repository<Unit>()
            .AnyAsync(u => u.Name == unitDto.Name && u.BuildingId == unitDto.BuildingId && u.UnitId != id);

        if (nameConflict)
            throw new ConflictException("Unit with the same name already exists in this building.");

        var oldStatus = existing.Status;
        var newStatus = unitDto.Status;

        if (oldStatus != newStatus)
        {
            // Reversions from Sold to Sale or Rented to Rent are allowed.

            // Path Separation: No crossing between Sale and Rent paths
            if ((oldStatus == UnitStatus.Sale || oldStatus == UnitStatus.Sold) && 
                (newStatus == UnitStatus.Rent || newStatus == UnitStatus.Rented))
                throw new BadRequestException("Cannot move unit from Sale path to Rent path.");

            if ((oldStatus == UnitStatus.Rent || oldStatus == UnitStatus.Rented) && 
                (newStatus == UnitStatus.Sale || newStatus == UnitStatus.Sold))
                throw new BadRequestException("Cannot move unit from Rent path to Sale path.");
        }

        mapper.Map(unitDto, existing);


        var currentFeatureIds = existing.UnitFeatures.Select(uf => uf.FeatureId).ToList();
        existing.UnitFeatures.Clear();
        if (unitDto.FeatureIds.Any())
        {
            var features = await unitOfWork.Repository<Feature>().GetAllAsync();
            foreach (var featureId in unitDto.FeatureIds)
            {
                var feature = features.FirstOrDefault(f => f.FeatureId == featureId);
                if (feature == null)
                    throw new NotFoundExpection("Feature", featureId);

                if (!feature.IsActive && !currentFeatureIds.Contains(featureId))
                    throw new BadRequestException($"الميزة '{feature.Name}' معطلة ولا يمكن إضافتها حالياً.");

                existing.UnitFeatures.Add(new UnitFeature { FeatureId = featureId });
            }
        }

        var currentInsuranceIds = existing.UnitInsurance.Select(ui => ui.InsuranceId).ToList();
        existing.UnitInsurance.Clear();
        if (unitDto.InsuranceIds.Any())
        {
            var insurances = await unitOfWork.Repository<Insurance>().GetAllAsync();
            foreach (var insuranceId in unitDto.InsuranceIds)
            {
                var insurance = insurances.FirstOrDefault(i => i.InsuranceId == insuranceId);
                if (insurance == null)
                    throw new NotFoundExpection("Insurance", insuranceId);

                if (!insurance.IsActive && !currentInsuranceIds.Contains(insuranceId))
                    throw new BadRequestException($"التأمين '{insurance.Name}' معطل ولا يمكن إضافته حالياً.");

                existing.UnitInsurance.Add(new UnitInsurance { InsuranceId = insuranceId });
            }
        }

        existing.NearbyFacilities.Clear();
        var nearbyFacilities = unitDto.NearbyFacilities.Any()
            ? DeduplicateNearbyFacilities(unitDto.NearbyFacilities.Select(dto => mapper.Map<NearbyFacility>(dto)))
            : DeduplicateNearbyFacilities(await FetchNearbyFacilitiesAsync(unitDto.Latitude, unitDto.Longitude));

        foreach (var facility in nearbyFacilities)
        {
            existing.NearbyFacilities.Add(facility);
        }

        unitOfWork.Repository<Unit>().Update(existing);

        var publicMediaToDelete = new List<(string PublicId, MediaType Type)>();

        // Process Media deletions during update
        if (unitDto.DeletedMediaIds != null && unitDto.DeletedMediaIds.Any())
        {
            var mediaToDelete = existing.Media.Where(m => unitDto.DeletedMediaIds.Contains(m.MediaId)).ToList();
            var mediaRepo = unitOfWork.Repository<UnitMedia>();
            foreach (var m in mediaToDelete)
            {
                if (!string.IsNullOrEmpty(m.PublicId))
                    publicMediaToDelete.Add((m.PublicId, m.Type));
                mediaRepo.Remove(m);
                existing.Media.Remove(m);
            }
        }

        // Process new Media
        var newlyUploadedImageIds = new List<string>();
        var newlyUploadedVideoIds = new List<string>();

        try
        {
            if (unitDto.ThumbnailImage != null)
            {
                var oldThumb = existing.Media.FirstOrDefault(media => media.IsThumbnail);
                if (oldThumb != null)
                {
                    if (!string.IsNullOrEmpty(oldThumb.PublicId)) publicMediaToDelete.Add((oldThumb.PublicId, oldThumb.Type));
                    unitOfWork.Repository<UnitMedia>().Remove(oldThumb);
                    existing.Media.Remove(oldThumb);
                }
                var res = await mediaService.UploadImageAsync(unitDto.ThumbnailImage);
                newlyUploadedImageIds.Add(res.PublicId);
                existing.Media.Add(new UnitMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Image, IsThumbnail = true });
            }

            if (unitDto.Images != null && unitDto.Images.Any())
            {
                foreach (var img in unitDto.Images)
                {
                    var res = await mediaService.UploadImageAsync(img);
                    newlyUploadedImageIds.Add(res.PublicId);
                    existing.Media.Add(new UnitMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Image, IsThumbnail = false });
                }
            }

            if (unitDto.Designs != null && unitDto.Designs.Any())
            {
                foreach (var img in unitDto.Designs)
                {
                    var res = await mediaService.UploadImageAsync(img);
                    newlyUploadedImageIds.Add(res.PublicId);
                    existing.Media.Add(new UnitMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Design, IsThumbnail = false });
                }
            }

            if (unitDto.VideoFile != null)
            {
                var oldVideo = existing.Media.FirstOrDefault(media => media.Type == MediaType.Video);
                if (oldVideo != null)
                {
                    if (!string.IsNullOrEmpty(oldVideo.PublicId)) publicMediaToDelete.Add((oldVideo.PublicId, oldVideo.Type));
                    unitOfWork.Repository<UnitMedia>().Remove(oldVideo);
                    existing.Media.Remove(oldVideo);
                }
                var res = await mediaService.UploadVideoAsync(unitDto.VideoFile);
                newlyUploadedVideoIds.Add(res.PublicId);
                existing.Media.Add(new UnitMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Video, IsThumbnail = false });
            }

            if (unitDto.Panorama360 != null)
            {
                var oldPano = existing.Media.FirstOrDefault(media => media.Type == MediaType.Panorama360);
                if (oldPano != null)
                {
                    if (!string.IsNullOrEmpty(oldPano.PublicId)) publicMediaToDelete.Add((oldPano.PublicId, oldPano.Type));
                    unitOfWork.Repository<UnitMedia>().Remove(oldPano);
                    existing.Media.Remove(oldPano);
                }
                var res = await mediaService.UploadImageAsync(unitDto.Panorama360);
                newlyUploadedImageIds.Add(res.PublicId);
                existing.Media.Add(new UnitMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Panorama360, IsThumbnail = false });
            }


            await unitOfWork.CompleteAsync();
        }
        catch (Exception)
        {
            foreach (var pid in newlyUploadedImageIds) await mediaService.DeleteImageAsync(pid);
            foreach (var pid in newlyUploadedVideoIds) await mediaService.DeleteVideoAsync(pid);
            throw;
        }

        foreach (var m in publicMediaToDelete)
        {
            if (m.Type == MediaType.Video)
                await mediaService.DeleteVideoAsync(m.PublicId);
            else
                await mediaService.DeleteImageAsync(m.PublicId);
        }

        var newProjectId = await GetProjectIdByBuildingIdAsync(existing.BuildingId);
        if (oldProjectId.HasValue)
            await RecalculateProjectStatusAsync(oldProjectId.Value);

        if (newProjectId.HasValue && newProjectId != oldProjectId)
            await RecalculateProjectStatusAsync(newProjectId.Value);

        var updated = await GetUnitByIdAsync(id);
        if (updated == null)
            throw new NotFoundExpection("Unit", id);

        return updated;
    }

    public async Task<GetUnitDto> DeleteUnitAsync(int id)
    {
        var spec = new UnitWithDetailsSpecification(id);
        var unit = await unitOfWork.Repository<Unit>().GetByIdAsync(spec);

        if (unit == null)
            throw new NotFoundExpection("Unit", id);


        var deletedDto = mapper.Map<GetUnitDto>(unit);
        var buildingId = unit.BuildingId;
        var projectId = await GetProjectIdByBuildingIdAsync(buildingId);

        var publicMediaToDelete = new List<(string PublicId, MediaType Type)>();

        // Cleanup Cloudinary and DB
        if (unit.Media.Count > 0)
        {
            var mediaRepo = unitOfWork.Repository<UnitMedia>();
            foreach (var m in unit.Media.ToList())
            {
                if (!string.IsNullOrEmpty(m.PublicId))
                {
                    publicMediaToDelete.Add((m.PublicId, m.Type));
                }
                mediaRepo.Remove(m);
            }
        }

        unitOfWork.Repository<Unit>().Remove(unit);
        await unitOfWork.CompleteAsync();

        foreach (var m in publicMediaToDelete)
        {
            if (m.Type == MediaType.Video)
                await mediaService.DeleteVideoAsync(m.PublicId);
            else
                await mediaService.DeleteImageAsync(m.PublicId);
        }

        // Keep in-memory object graph consistent
        var buildingRef = await unitOfWork.Repository<Building>().GetByIdAsync(buildingId);
        if (buildingRef != null)
        {
            var toRemove = buildingRef.Units.FirstOrDefault(u => u.UnitId == id);
            if (toRemove != null)
                buildingRef.Units.Remove(toRemove);
        }

        if (projectId.HasValue)
            await RecalculateProjectStatusAsync(projectId.Value);

        return deletedDto;
    }

    public async Task<IReadOnlyList<UnitListDto>> GetBuildingUnitsAsync(int buildingId)
    {
        var exists = await unitOfWork.Repository<Building>()
            .AnyAsync(b => b.BuildingId == buildingId);


        if (!exists)
            throw new NotFoundExpection("Building", buildingId);

        var spec = new UnitsByBuildingSpecification(buildingId);
        var units = await unitOfWork.Repository<Unit>().GetAllAsync(spec);

        return mapper.Map<IReadOnlyList<UnitListDto>>(units);
    }

    public async Task<IReadOnlyList<UnitListDto>> GetProjectUnitsAsync(int projectId)
    {
        var exists = await unitOfWork.Repository<Project>()
            .AnyAsync(p => p.ProjectId == projectId);

        if (!exists)
            throw new NotFoundExpection("Project", projectId);

        var spec = new UnitsByProjectSpecification(projectId);
        var units = await unitOfWork.Repository<Unit>().GetAllAsync(spec);

        return mapper.Map<IReadOnlyList<UnitListDto>>(units);
    }

    private async Task<int?> GetProjectIdByBuildingIdAsync(int buildingId)
    {
        var building = await unitOfWork.Repository<Building>().GetByIdAsync(buildingId);
        return building?.ProjectId;
    }

    private async Task RecalculateProjectStatusAsync(int projectId)
    {
        var projectSpec = new ProjectWithBuildingsSpecification(projectId);
        var project = await unitOfWork.Repository<Project>().GetByIdAsync(projectSpec);
        if (project == null) return;

        var allUnits = project.Buildings.SelectMany(b => b.Units).ToList();

        // 1. Update each building's derived status
        foreach (var building in project.Buildings)
        {
            building.Status = ProjectLogicHelpers.DeriveBuildingStatus(building.Type, building.Units);
            if (building.Units.Any())
            {
                building.FloorCount = building.Units.Max(u => u.Floor);
            }
            unitOfWork.Repository<Building>().Update(building);
        }

        // 2. Update project status
        var oldStatus = project.Status;
        project.Status = ProjectLogicHelpers.DeriveProjectStatus(allUnits, project.Status);


        project.AvailableUnitsCount  = allUnits.Count(u => u.Status == UnitStatus.Sale   || u.Status == UnitStatus.Rent);
        project.TransactedUnitsCount = allUnits.Count(u => u.Status == UnitStatus.Sold   || u.Status == UnitStatus.Rented);
        project.TotalBuildingArea    = project.Buildings.Sum(b => b.BuildingArea);

        unitOfWork.Repository<Project>().Update(project);
        await unitOfWork.CompleteAsync();
    }


    private void ValidateMediaFiles(Microsoft.AspNetCore.Http.IFormFile? thumbnail, List<Microsoft.AspNetCore.Http.IFormFile>? images, List<Microsoft.AspNetCore.Http.IFormFile>? designs, Microsoft.AspNetCore.Http.IFormFile? panorama, Microsoft.AspNetCore.Http.IFormFile? video)
    {
        var validImageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        var validVideoTypes = new[] { "video/mp4", "video/avi", "video/mpeg", "video/quicktime" };

        if (thumbnail != null && !validImageTypes.Contains(thumbnail.ContentType.ToLower()))
            throw new BadRequestException("Thumbnail must be a valid image format (JPEG, PNG, GIF, WEBP).");

        if (images != null)
            foreach (var img in images)
                if (!validImageTypes.Contains(img.ContentType.ToLower()))
                    throw new BadRequestException("One or more images have an invalid format. Only images are allowed.");

        if (designs != null)
            foreach (var img in designs)
                if (!validImageTypes.Contains(img.ContentType.ToLower()))
                    throw new BadRequestException("One or more designs have an invalid format. Only images are allowed.");

        if (panorama != null && !validImageTypes.Contains(panorama.ContentType.ToLower()))
            throw new BadRequestException("Panorama must be a valid image format.");

        if (video != null && !validVideoTypes.Contains(video.ContentType.ToLower()))
            throw new BadRequestException("Video must be a valid video format (MP4, AVI, MPEG, MOV).");
    }

    private async Task<List<NearbyFacility>> FetchNearbyFacilitiesAsync(double latitude, double longitude)
    {
        try
        {
            var query = BuildOverpassQuery(latitude, longitude, overpassRadiusMeters);
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["data"] = query
            });

            var response = await httpClient.PostAsync(overpassUrl, content);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var facilities = new List<NearbyFacility>();
            if (!doc.RootElement.TryGetProperty("elements", out var elements))
                return facilities;

            foreach (var element in elements.EnumerateArray())
            {
                if (!element.TryGetProperty("tags", out var tags))
                    continue;

                var facilityType = TryMapFacilityType(tags);
                if (facilityType == null)
                    continue;

                var (lat, lng) = GetElementCoordinates(element);
                if (!lat.HasValue || !lng.HasValue)
                    continue;

                var name = GetTagValue(tags, $"name:{overpassLanguage}")
                           ?? GetTagValue(tags, "name")
                           ?? facilityType.Value.ToString();

                var distanceMeters = CalculateDistanceMeters(latitude, longitude, lat.Value, lng.Value);
                var distanceText = FormatDistance(distanceMeters);

                facilities.Add(new NearbyFacility
                {
                    Name = name,
                    Type = facilityType.Value,
                    Distance = distanceText,
                    Latitude = lat,
                    Longitude = lng,
                    Area = 0
                });
            }

            return DeduplicateNearbyFacilities(facilities)
                .OrderBy(f => ParseDistanceMeters(f.Distance))
                .Take(overpassMaxResults)
                .ToList();
        }
        catch
        {
            return new List<NearbyFacility>();
        }
    }

    private string BuildOverpassQuery(double latitude, double longitude, int radiusMeters)
    {
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lng = longitude.ToString(CultureInfo.InvariantCulture);
        var radius = radiusMeters.ToString(CultureInfo.InvariantCulture);

        return $"[out:json][timeout:25];(\n" +
               $"node[\"amenity\"=\"mosque\"](around:{radius},{lat},{lng});\n" +
               $"node[\"amenity\"=\"school\"](around:{radius},{lat},{lng});\n" +
               $"node[\"amenity\"=\"hospital\"](around:{radius},{lat},{lng});\n" +
               $"node[\"amenity\"=\"restaurant\"](around:{radius},{lat},{lng});\n" +
               $"node[\"amenity\"=\"bank\"](around:{radius},{lat},{lng});\n" +
               $"node[\"amenity\"=\"pharmacy\"](around:{radius},{lat},{lng});\n" +
               $"node[\"shop\"=\"supermarket\"](around:{radius},{lat},{lng});\n" +
               $"node[\"leisure\"=\"park\"](around:{radius},{lat},{lng});\n" +
               $"node[\"leisure\"=\"club\"](around:{radius},{lat},{lng});\n" +
               $"way[\"amenity\"=\"mosque\"](around:{radius},{lat},{lng});\n" +
               $"way[\"amenity\"=\"school\"](around:{radius},{lat},{lng});\n" +
               $"way[\"amenity\"=\"hospital\"](around:{radius},{lat},{lng});\n" +
               $"way[\"amenity\"=\"restaurant\"](around:{radius},{lat},{lng});\n" +
               $"way[\"amenity\"=\"bank\"](around:{radius},{lat},{lng});\n" +
               $"way[\"amenity\"=\"pharmacy\"](around:{radius},{lat},{lng});\n" +
               $"way[\"shop\"=\"supermarket\"](around:{radius},{lat},{lng});\n" +
               $"way[\"leisure\"=\"park\"](around:{radius},{lat},{lng});\n" +
               $"way[\"leisure\"=\"club\"](around:{radius},{lat},{lng});\n" +
               $"relation[\"amenity\"=\"mosque\"](around:{radius},{lat},{lng});\n" +
               $"relation[\"amenity\"=\"school\"](around:{radius},{lat},{lng});\n" +
               $"relation[\"amenity\"=\"hospital\"](around:{radius},{lat},{lng});\n" +
               $"relation[\"amenity\"=\"restaurant\"](around:{radius},{lat},{lng});\n" +
               $"relation[\"amenity\"=\"bank\"](around:{radius},{lat},{lng});\n" +
               $"relation[\"amenity\"=\"pharmacy\"](around:{radius},{lat},{lng});\n" +
               $"relation[\"shop\"=\"supermarket\"](around:{radius},{lat},{lng});\n" +
               $"relation[\"leisure\"=\"park\"](around:{radius},{lat},{lng});\n" +
               $"relation[\"leisure\"=\"club\"](around:{radius},{lat},{lng});\n" +
               ");out center tags;";
    }

    private static (double? lat, double? lng) GetElementCoordinates(JsonElement element)
    {
        if (element.TryGetProperty("lat", out var lat) && element.TryGetProperty("lon", out var lon))
            return (lat.GetDouble(), lon.GetDouble());

        if (element.TryGetProperty("center", out var center) &&
            center.TryGetProperty("lat", out var centerLat) &&
            center.TryGetProperty("lon", out var centerLon))
            return (centerLat.GetDouble(), centerLon.GetDouble());

        return (null, null);
    }

    private static string? GetTagValue(JsonElement tags, string key)
    {
        if (tags.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
            return value.GetString();
        return null;
    }

    private static FacilityType? TryMapFacilityType(JsonElement tags)
    {
        var amenity = GetTagValue(tags, "amenity");
        if (amenity == "mosque") return FacilityType.Mosque;
        if (amenity == "school") return FacilityType.School;
        if (amenity == "hospital") return FacilityType.Hospital;
        if (amenity == "restaurant") return FacilityType.Restaurant;
        if (amenity == "bank") return FacilityType.Bank;
        if (amenity == "pharmacy") return FacilityType.Pharmacy;

        var shop = GetTagValue(tags, "shop");
        if (shop == "supermarket") return FacilityType.SuperMarket;

        var leisure = GetTagValue(tags, "leisure");
        if (leisure == "park") return FacilityType.Park;
        if (leisure == "club") return FacilityType.Club;

        return null;
    }

    private static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double radius = 6371000;
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return radius * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);

    private static string FormatDistance(double meters)
    {
        if (meters < 1000)
            return $"{Math.Round(meters)} متر";
        return $"{Math.Round(meters / 1000.0, 1).ToString("0.0", CultureInfo.InvariantCulture)} كم";
    }

    private static double ParseDistanceMeters(string? distance)
    {
        if (string.IsNullOrWhiteSpace(distance)) return double.MaxValue;
        if (distance.Contains("كم"))
        {
            var num = distance.Replace("كم", string.Empty).Trim();
            return double.TryParse(num, NumberStyles.Any, CultureInfo.InvariantCulture, out var km)
                ? km * 1000
                : double.MaxValue;
        }
        var meters = distance.Replace("متر", string.Empty).Trim();
        return double.TryParse(meters, NumberStyles.Any, CultureInfo.InvariantCulture, out var m)
            ? m
            : double.MaxValue;
    }

    private static List<NearbyFacility> DeduplicateNearbyFacilities(IEnumerable<NearbyFacility> facilities)
    {
        return facilities
            .Where(f => f != null)
            .GroupBy(f => new
            {
                Name = (f.Name ?? string.Empty).Trim().ToLowerInvariant(),
                Type = f.Type,
                Distance = (f.Distance ?? string.Empty).Trim().ToLowerInvariant(),
                Latitude = f.Latitude.HasValue ? Math.Round(f.Latitude.Value, 6) : (double?)null,
                Longitude = f.Longitude.HasValue ? Math.Round(f.Longitude.Value, 6) : (double?)null,
            })
            .Select(g => g.First())
            .ToList();
    }
}
