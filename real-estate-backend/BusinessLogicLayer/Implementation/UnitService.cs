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


        if (unitDto.Area <= 0)
            throw new BadRequestException("Unit area must be greater than 0.");

        if (unitDto.Area > building.MaxArea)
            throw new BadRequestException($"Unit area ({unitDto.Area}) exceeds the building's designated maximum area ({building.MaxArea}).");


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

        if (unitDto.NearbyFacilities.Any())
        {
            foreach (var facilityDto in unitDto.NearbyFacilities)
            {
                var facility = mapper.Map<NearbyFacility>(facilityDto);
                unit.NearbyFacilities.Add(facility);
            }
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

        if (unitDto.Area <= 0)
            throw new BadRequestException("Unit area data is invalid.");

        if (unitDto.Area > targetBuilding.MaxArea)
            throw new BadRequestException("Unit area exceeds the maximum allowed area for a floor in this building.");

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
            // 1. Terminal Check: Sold cannot go back to Sale
            if (oldStatus == UnitStatus.Sold && newStatus == UnitStatus.Sale)
            {
                throw new BadRequestException("لا يمكن تحويل الوحدة من حالة 'مباعة' (Sold) إلى 'للبيع' (Sale) مرة أخرى.");
            }

            // 2. Path Separation: No crossing between Sale and Rent paths
            if ((oldStatus == UnitStatus.Sale || oldStatus == UnitStatus.Sold) && 
                (newStatus == UnitStatus.Rent || newStatus == UnitStatus.Rented))
            {
                throw new BadRequestException($"لا يمكن تحويل الوحدة من مسار البيع ({oldStatus}) إلى مسار الإيجار ({newStatus}).");
            }

            if ((oldStatus == UnitStatus.Rent || oldStatus == UnitStatus.Rented) && 
                (newStatus == UnitStatus.Sale || newStatus == UnitStatus.Sold))
            {
                throw new BadRequestException($"لا يمكن تحويل الوحدة من مسار الإيجار ({oldStatus}) إلى مسار البيع ({newStatus}).");
            }
        }

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

        existing.NearbyFacilities.Clear();
        if (unitDto.NearbyFacilities.Any())
        {
            foreach (var facilityDto in unitDto.NearbyFacilities)
            {
                var facility = mapper.Map<NearbyFacility>(facilityDto);
                existing.NearbyFacilities.Add(facility);
            }
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

        if (unit.Status is UnitStatus.Sold or UnitStatus.Rented)
            throw new BadRequestException("Cannot delete a unit that has been sold or rented.");


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

    private static ProjectStatus DeriveProjectStatus(IEnumerable<Unit> units, ProjectStatus currentStatus)
    {
        return BusinessLogicLayer.Helpers.ProjectLogicHelpers.DeriveProjectStatus(units, currentStatus);
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
}
