namespace BusinessLogicLayer.Implementation;

public class UnitService(IUnitOfWork unitOfWork, IMapper mapper, IMediaService mediaService) : IUnitService
{
    public async Task<PaginatedResult<UnitListDto>> GetUnitsAsync(UnitSpecParams @params)
    {
        var spec = new UnitWithDetailsSpecification(@params);
        var countSpec = new UnitWithFiltersForCountSpecification(@params);

        var total = await unitOfWork.Repository<Unit>().CountAsync(countSpec);
        var units = await unitOfWork.Repository<Unit>().GetAllAsync(spec);

        var data = mapper.Map<IReadOnlyList<UnitListDto>>(units);
        return new PaginatedResult<UnitListDto>(@params.Page, @params.PageSize, total, data);
    }

    public async Task<GetUnitDto?> GetUnitByIdAsync(int id)
    {
        var spec = new UnitWithDetailsSpecification(id);
        var unit = await unitOfWork.Repository<Unit>().GetByIdAsync(spec);

        return unit == null ? null : mapper.Map<GetUnitDto>(unit);
    }

    public async Task<GetUnitDto> CreateUnitAsync(CreateUnitDto unitDto)
    {
        var building = await unitOfWork.Repository<Building>()
            .GetByIdAsync(unitDto.BuildingId);

        if (building == null)
            throw new NotFoundExpection("Building", unitDto.BuildingId);


        var project = await unitOfWork.Repository<Project>().GetByIdAsync(building.ProjectId);
        if (project == null)
            throw new NotFoundExpection("Project", building.ProjectId);


        bool isSaleProject = project.Status == ProjectStatus.Sale || project.Status == ProjectStatus.Sold;

        if (isSaleProject && (unitDto.Status == UnitStatus.Rent || unitDto.Status == UnitStatus.Rented))
            throw new BadRequestException("Unit status must be Sale or Sold because the project is Sale-type.");

        if (!isSaleProject && (unitDto.Status == UnitStatus.Sale || unitDto.Status == UnitStatus.Sold))
            throw new BadRequestException("Unit status must be Rent or Rented because the project is Rent-type.");


        if (unitDto.Type == UnitType.Villa)
        {
            var hasVillaInOtherBuilding = await unitOfWork.Repository<Unit>()
                .AnyAsync(u => u.Type == UnitType.Villa && u.Building.ProjectId == building.ProjectId && u.BuildingId != unitDto.BuildingId);
            
            if (hasVillaInOtherBuilding)
                throw new BadRequestException("Villa type units can only exist in one building per project.");
        }


        if (building.MaxArea.HasValue && unitDto.Area > building.MaxArea.Value)
            throw new BadRequestException($"Unit area ({unitDto.Area}) exceeds the building's designated maximum area for floor {unitDto.Floor} ({building.MaxArea.Value}).");


        var exists = await unitOfWork.Repository<Unit>()
            .AnyAsync(u => u.Name == unitDto.Name && u.BuildingId == unitDto.BuildingId);

        if (exists)
            throw new ConflictException("Unit with the same name already exists in this building.");

        var unit = mapper.Map<Unit>(unitDto);
        unit.IsStatusChanged = false; // Cannot be set during create



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

        await unitOfWork.Repository<Unit>().AddAsync(unit);
        await unitOfWork.CompleteAsync();

        // Process Media
        if (unitDto.ThumbnailImage != null)
        {
            var url = await mediaService.UploadImageAsync(unitDto.ThumbnailImage);
            var media = new UnitMedia { UnitId = unit.UnitId, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Image, IsThumbnail = true };
            await unitOfWork.Repository<UnitMedia>().AddAsync(media);
        }

        if (unitDto.Images != null && unitDto.Images.Any())
        {
            foreach (var img in unitDto.Images)
            {
                var url = await mediaService.UploadImageAsync(img);
                var media = new UnitMedia { UnitId = unit.UnitId, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Image, IsThumbnail = false };
                await unitOfWork.Repository<UnitMedia>().AddAsync(media);
            }
        }

        if (unitDto.Designs != null && unitDto.Designs.Any())
        {
            foreach (var img in unitDto.Designs)
            {
                var url = await mediaService.UploadImageAsync(img);
                var media = new UnitMedia { UnitId = unit.UnitId, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Design, IsThumbnail = false };
                await unitOfWork.Repository<UnitMedia>().AddAsync(media);
            }
        }

        if (unitDto.Video != null)
        {
            var url = await mediaService.UploadImageAsync(unitDto.Video);
            var media = new UnitMedia { UnitId = unit.UnitId, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Video, IsThumbnail = false };
            await unitOfWork.Repository<UnitMedia>().AddAsync(media);
        }

        if (unitDto.Panorama360 != null)
        {
            var url = await mediaService.UploadImageAsync(unitDto.Panorama360);
            var media = new UnitMedia { UnitId = unit.UnitId, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Panorama360, IsThumbnail = false };
            await unitOfWork.Repository<UnitMedia>().AddAsync(media);
        }

        await unitOfWork.CompleteAsync();

        await RecalculateProjectStatusAsync(building.ProjectId);

        var created = await GetUnitByIdAsync(unit.UnitId);
        if (created == null)
            throw new NotFoundExpection("Unit", unit.UnitId);

        return created;
    }

    public async Task<GetUnitDto> UpdateUnitAsync(int id, UpdateUnitDto unitDto)
    {
        var spec = new UnitWithDetailsSpecification(id);
        var existing = await unitOfWork.Repository<Unit>().GetByIdAsync(spec);

        if (existing == null)
            throw new NotFoundExpection("Unit", id);

        var oldBuildingId = existing.BuildingId;
        var oldProjectId = await GetProjectIdByBuildingIdAsync(oldBuildingId);

        var targetBuilding = await unitOfWork.Repository<Building>().GetByIdAsync(unitDto.BuildingId);
        if (targetBuilding == null)
            throw new NotFoundExpection("Building", unitDto.BuildingId);


        var project = await unitOfWork.Repository<Project>().GetByIdAsync(targetBuilding.ProjectId);
        if (project == null)
            throw new NotFoundExpection("Project", targetBuilding.ProjectId);


        bool isSaleProject = project.Status == ProjectStatus.Sale || project.Status == ProjectStatus.Sold;

        if (isSaleProject && (unitDto.Status == UnitStatus.Rent || unitDto.Status == UnitStatus.Rented))
            throw new BadRequestException("Unit status must be Sale or Sold because the project is Sale-type.");

        if (!isSaleProject && (unitDto.Status == UnitStatus.Sale || unitDto.Status == UnitStatus.Sold))
            throw new BadRequestException("Unit status must be Rent or Rented because the project is Rent-type.");


        if (unitDto.Type == UnitType.Villa)
        {
            var hasVilla = await unitOfWork.Repository<Unit>()
                .AnyAsync(u => u.Type == UnitType.Villa && u.Building.ProjectId == targetBuilding.ProjectId && u.UnitId != id);
            
            if (hasVilla)
                throw new BadRequestException("This project already has a Villa.");
        }

        if (targetBuilding.MaxArea.HasValue && unitDto.Area > targetBuilding.MaxArea.Value)
            throw new BadRequestException("Unit area exceeds the maximum allowed area for a floor in this building.");

        if (existing.Status is UnitStatus.Sold or UnitStatus.Rented && unitDto.BuildingId != oldBuildingId)
            throw new BadRequestException("This unit cannot be moved because it has already been transacted (Sold/Rented).");


        var nameConflict = await unitOfWork.Repository<Unit>()
            .AnyAsync(u => u.Name == unitDto.Name && u.BuildingId == unitDto.BuildingId && u.UnitId != id);

        if (nameConflict)
            throw new ConflictException("Unit with the same name already exists in this building.");

        var oldStatus = existing.Status;
        mapper.Map(unitDto, existing);
        if (oldStatus != existing.Status)
            existing.IsStatusChanged = true;
        
        // Ensure explicit override from DTO if provided (since it's only modifiable in Edit)
        if (unitDto.IsStatusChanged) 
            existing.IsStatusChanged = true;
        else if (existing.IsStatusChanged && !unitDto.IsStatusChanged)
            existing.IsStatusChanged = false; // User can manually reset it


        existing.UnitFeatures.Clear();
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

                existing.UnitFeatures.Add(new UnitFeature { FeatureId = featureId });
            }
        }

        existing.UnitInsurance.Clear();
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

                existing.UnitInsurance.Add(new UnitInsurance { InsuranceId = insuranceId });
            }
        }

        unitOfWork.Repository<Unit>().Update(existing);

        // Process Media deletions during update
        if (unitDto.DeletedMediaIds != null && unitDto.DeletedMediaIds.Any())
        {
            var mediaToDelete = existing.Media.Where(m => unitDto.DeletedMediaIds.Contains(m.MediaId)).ToList();
            foreach (var m in mediaToDelete)
            {
                if (!string.IsNullOrEmpty(m.PublicId))
                    await mediaService.DeleteImageAsync(m.PublicId);
                unitOfWork.Repository<UnitMedia>().Remove(m);
            }
        }

        // Replace Thumbnail
        if (unitDto.ThumbnailImage != null)
        {
            var oldThumb = existing.Media.FirstOrDefault(m => m.IsThumbnail);
            if (oldThumb != null)
            {
                if (!string.IsNullOrEmpty(oldThumb.PublicId)) await mediaService.DeleteImageAsync(oldThumb.PublicId);
                unitOfWork.Repository<UnitMedia>().Remove(oldThumb);
            }

            var url = await mediaService.UploadImageAsync(unitDto.ThumbnailImage);
            var media = new UnitMedia { UnitId = id, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Image, IsThumbnail = true };
            await unitOfWork.Repository<UnitMedia>().AddAsync(media);
        }

        // Append new images
        if (unitDto.Images != null && unitDto.Images.Any())
        {
            foreach (var img in unitDto.Images)
            {
                var url = await mediaService.UploadImageAsync(img);
                var media = new UnitMedia { UnitId = id, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Image, IsThumbnail = false };
                await unitOfWork.Repository<UnitMedia>().AddAsync(media);
            }
        }

        // Append new designs
        if (unitDto.Designs != null && unitDto.Designs.Any())
        {
            foreach (var img in unitDto.Designs)
            {
                var url = await mediaService.UploadImageAsync(img);
                var media = new UnitMedia { UnitId = id, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Design, IsThumbnail = false };
                await unitOfWork.Repository<UnitMedia>().AddAsync(media);
            }
        }

        // Replace Video
        if (unitDto.Video != null)
        {
            var oldVideo = existing.Media.FirstOrDefault(m => m.Type == MediaType.Video);
            if (oldVideo != null)
            {
                if (!string.IsNullOrEmpty(oldVideo.PublicId)) await mediaService.DeleteImageAsync(oldVideo.PublicId);
                unitOfWork.Repository<UnitMedia>().Remove(oldVideo);
            }

            var url = await mediaService.UploadImageAsync(unitDto.Video);
            var media = new UnitMedia { UnitId = id, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Video, IsThumbnail = false };
            await unitOfWork.Repository<UnitMedia>().AddAsync(media);
        }

        // Replace Panorama
        if (unitDto.Panorama360 != null)
        {
            var oldPano = existing.Media.FirstOrDefault(m => m.Type == MediaType.Panorama360);
            if (oldPano != null)
            {
                if (!string.IsNullOrEmpty(oldPano.PublicId)) await mediaService.DeleteImageAsync(oldPano.PublicId);
                unitOfWork.Repository<UnitMedia>().Remove(oldPano);
            }

            var url = await mediaService.UploadImageAsync(unitDto.Panorama360);
            var media = new UnitMedia { UnitId = id, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Panorama360, IsThumbnail = false };
            await unitOfWork.Repository<UnitMedia>().AddAsync(media);
        }

        await unitOfWork.CompleteAsync();

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

        if (unit.Status is UnitStatus.Sold or UnitStatus.Rented)
            throw new BadRequestException("Cannot delete a unit that has been sold or rented.");


        var deletedDto = mapper.Map<GetUnitDto>(unit);
        var buildingId = unit.BuildingId;
        var projectId = await GetProjectIdByBuildingIdAsync(buildingId);

        // Cleanup Cloudinary
        foreach (var m in unit.Media)
        {
            if (!string.IsNullOrEmpty(m.PublicId))
            {
                await mediaService.DeleteImageAsync(m.PublicId);
            }
        }

        unitOfWork.Repository<Unit>().Remove(unit);
        await unitOfWork.CompleteAsync();

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

    private async Task<int?> GetProjectIdByBuildingIdAsync(int buildingId)
    {
        var building = await unitOfWork.Repository<Building>().GetByIdAsync(buildingId);
        return building?.ProjectId;
    }

    private async Task RecalculateProjectStatusAsync(int projectId)
    {
        var projectSpec = new ProjectWithBuildingsSpecification(projectId);
        var project = await unitOfWork.Repository<Project>().GetByIdAsync(projectSpec);
        if (project == null)
            return;

        var activeBuildings = project.Buildings.ToList();

        var allUnits = activeBuildings.SelectMany(b => b.Units).ToList();

        var oldStatus = project.Status;
        project.Status = DeriveProjectStatus(allUnits, project.Status);
        
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

        
        // Area Calculations
        // TotalBuildingArea = Σ (BuildingArea_i) + Σ (Max unit area in Building_i)
        project.TotalBuildingArea = activeBuildings.Sum(b => (b.BuildingArea ?? 0) + (b.Units.Any() ? b.Units.Max(u => u.Area ?? 0) : 0));

        unitOfWork.Repository<Project>().Update(project);
        await unitOfWork.CompleteAsync();

    }

    private static ProjectStatus DeriveProjectStatus(IEnumerable<Unit> units, ProjectStatus currentStatus)
    {
        return BusinessLogicLayer.Helpers.ProjectLogicHelpers.DeriveProjectStatus(units, currentStatus);
    }

}
