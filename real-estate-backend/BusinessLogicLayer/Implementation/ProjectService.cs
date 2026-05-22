namespace BusinessLogicLayer.Implementation;

public class ProjectService(IUnitOfWork unitOfWork, IMapper mapper, IMediaService mediaService) : IProjectService
{
    public async Task<PaginatedResult<ProjectListDto>> GetProjectsAsync(ProjectSpecParams @params)
    {
        var spec = new ProjectWithBuildingsSpecification(@params);
        var countSpec = new ProjectWithBuildingsSpecification(@params, true);

        var totalItems = await unitOfWork.Repository<Project>().CountAsync(countSpec);
        var projects = await unitOfWork.Repository<Project>().GetAllAsync(spec);

        var data = mapper.Map<IReadOnlyList<ProjectListDto>>(projects);

        return new PaginatedResult<ProjectListDto>(@params.Page, @params.PageSize, totalItems, data);
    }

    public async Task<ProjectDetailsDto?> GetProjectByIdAsync(int id)
    {
        var spec = new ProjectWithBuildingsSpecification(id);
        var project = await unitOfWork.Repository<Project>().GetByIdAsync(spec);

        if (project == null)
            return null;

        var dto = mapper.Map<ProjectDetailsDto>(project);
        
        // 1. Calculate Buildings count directly from DB
        dto.BuildingsNumber = await unitOfWork.Repository<Building>().CountAsync(new BuildingsByProjectSpecification(id));
        
        // 2. Calculate Units count, rooms, halls, min/max price from units in project
        var unitSpec = new UnitsByProjectSpecification(id);
        var units = await unitOfWork.Repository<Unit>().GetAllAsync(unitSpec);
        
        dto.UnitsNumber = units.Count();
        dto.AvailableUnitsCount = units.Count(u => u.Status == UnitStatus.Sale || u.Status == UnitStatus.Rent);
        dto.TransactedUnitsCount = units.Count(u => u.Status == UnitStatus.Sold || u.Status == UnitStatus.Rented);
        
        if (units.Any())
        {
            dto.MinPrice = units.Min(u => u.Price);
            dto.MaxPrice = units.Max(u => u.Price);
            dto.TotalRooms = units.Sum(u => u.Rooms);
            dto.TotalHalls = units.Sum(u => u.Salons);
        }
        else
        {
            dto.MinPrice = 0;
            dto.MaxPrice = 0;
            dto.TotalRooms = 0;
            dto.TotalHalls = 0;
        }

        // 3. Clear buildings list completely as requested!
        dto.Buildings = new List<BuildingDto>();

        return dto;
    }

    public async Task<ProjectDetailsDto> CreateProjectAsync(ProjectCreateDto projectDto)
    {
        ValidateMediaFiles(projectDto.ThumbnailImage, projectDto.Images, projectDto.Panorama360, projectDto.VideoFile);

        // 1. Initial Validation
        var initialStatus = projectDto.Status is ProjectStatus.Sale or ProjectStatus.Rent
            ? projectDto.Status
            : ProjectStatus.Sale;
        projectDto.Status = initialStatus;

        if (projectDto.BuildUpArea > projectDto.LandArea)
            throw new BadRequestException("Build-up area must be less than or equal to Land area.");

        if (projectDto.TotalBuildingArea.HasValue && projectDto.TotalBuildingArea > projectDto.BuildUpArea)
            throw new BadRequestException("Total building area must be less than or equal to Build-up area.");

        var exists = await unitOfWork.Repository<Project>()
            .AnyAsync(p => p.Name == projectDto.Name);

        if (exists)
            throw new ConflictException("Project with the same name already exists.");

        var project = mapper.Map<Project>(projectDto);

        if (projectDto.FeatureIds.Any())
        {
            var features = await unitOfWork.Repository<Feature>().GetAllAsync();
            foreach (var fId in projectDto.FeatureIds)
            {
                var feature = features.FirstOrDefault(f => f.FeatureId == fId);
                if (feature == null) throw new NotFoundExpection("Feature", fId);
                if (!feature.IsActive) throw new BadRequestException($"الميزة '{feature.Name}' معطلة ولا يمكن إضافتها حالياً.");
                project.ProjectFeatures.Add(new ProjectFeature { FeatureId = fId });
            }
        }

        if (projectDto.InsuranceIds.Any())
        {
            var insurances = await unitOfWork.Repository<Insurance>().GetAllAsync();
            foreach (var iId in projectDto.InsuranceIds)
            {
                var insurance = insurances.FirstOrDefault(i => i.InsuranceId == iId);
                if (insurance == null) throw new NotFoundExpection("Insurance", iId);
                if (!insurance.IsActive) throw new BadRequestException($"التأمين '{insurance.Name}' معطل ولا يمكن إضافته حالياً.");
                project.ProjectInsurance.Add(new ProjectInsurance { InsuranceId = iId });
            }
        }

        var newlyUploadedImageIds = new List<string>();
        var newlyUploadedVideoIds = new List<string>();

        try
        {
            if (projectDto.ThumbnailImage != null)
            {
                var res = await mediaService.UploadImageAsync(projectDto.ThumbnailImage);
                newlyUploadedImageIds.Add(res.PublicId);
                project.Media.Add(new ProjectMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Image, IsThumbnail = true });
            }

            if (projectDto.Images != null && projectDto.Images.Any())
            {
                foreach (var img in projectDto.Images)
                {
                    var res = await mediaService.UploadImageAsync(img);
                    newlyUploadedImageIds.Add(res.PublicId);
                    project.Media.Add(new ProjectMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Image, IsThumbnail = false });
                }
            }

            if (projectDto.Panorama360 != null)
            {
                var res = await mediaService.UploadImageAsync(projectDto.Panorama360);
                newlyUploadedImageIds.Add(res.PublicId);
                project.Media.Add(new ProjectMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Panorama360, IsThumbnail = false });
            }

            if (projectDto.VideoFile != null)
            {
                var res = await mediaService.UploadVideoAsync(projectDto.VideoFile);
                newlyUploadedVideoIds.Add(res.PublicId);
                project.Media.Add(new ProjectMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Video, IsThumbnail = false });
            }

            await unitOfWork.Repository<Project>().AddAsync(project);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception)
        {
            foreach (var pid in newlyUploadedImageIds) await mediaService.DeleteImageAsync(pid);
            foreach (var pid in newlyUploadedVideoIds) await mediaService.DeleteVideoAsync(pid);
            throw;
        }

        var created = await GetProjectByIdAsync(project.ProjectId);
        if (created == null)
            throw new NotFoundExpection("Project", project.ProjectId);

        return created;
    }

    public async Task<ProjectDetailsDto> UpdateProjectAsync(int id, ProjectUpdateDto projectDto)
    {
        ValidateMediaFiles(projectDto.ThumbnailImage, projectDto.Images, projectDto.Panorama360, projectDto.VideoFile);

        var spec = new ProjectWithBuildingsSpecification(id);
        var existing = await unitOfWork.Repository<Project>().GetByIdAsync(spec);

        if (existing == null)
            throw new NotFoundExpection("Project", id);



        var exists = await unitOfWork.Repository<Project>()
            .AnyAsync(p => p.Name == projectDto.Name && p.ProjectId != id);

        if (exists)
            throw new ConflictException("Project with the same name already exists.");

        var oldStatus = existing.Status;
        var newStatus = projectDto.Status;

        // Load project with buildings and units to evaluate restrictions
        var projectWithUnits = await unitOfWork.Repository<Project>().GetByIdAsync(new ProjectWithBuildingsSpecification(id));
        var projectUnits = projectWithUnits?.Buildings.SelectMany(b => b.Units).ToList() ?? new List<Unit>();

        // Requirement: Project status can only be changed on update when the project does NOT have any buildings.
        // If there are buildings, disallow any status change (even within the same path).
        if (oldStatus != newStatus)
        {
            if (projectWithUnits != null && projectWithUnits.Buildings.Any())
            {
                throw new BadRequestException("لا يمكن تغيير حالة المشروع بعد إضافة مبانٍ. قم بحذف المباني أولاً إن أردت تغيير المسار.");
            }
        }

        mapper.Map(projectDto, existing);


        var currentFeatureIds = existing.ProjectFeatures.Select(pf => pf.FeatureId).ToList();
        existing.ProjectFeatures.Clear();
        if (projectDto.FeatureIds.Any())
        {
            var features = await unitOfWork.Repository<Feature>().GetAllAsync();
            foreach (var fId in projectDto.FeatureIds)
            {
                var feature = features.FirstOrDefault(f => f.FeatureId == fId);
                if (feature == null) throw new NotFoundExpection("Feature", fId);
                
                if (!feature.IsActive && !currentFeatureIds.Contains(fId)) 
                    throw new BadRequestException($"الميزة '{feature.Name}' معطلة ولا يمكن إضافتها حالياً.");
                
                existing.ProjectFeatures.Add(new ProjectFeature { FeatureId = fId });
            }
        }

        var currentInsuranceIds = existing.ProjectInsurance.Select(pi => pi.InsuranceId).ToList();
        existing.ProjectInsurance.Clear();
        if (projectDto.InsuranceIds.Any())
        {
            var insurances = await unitOfWork.Repository<Insurance>().GetAllAsync();
            foreach (var iId in projectDto.InsuranceIds)
            {
                var insurance = insurances.FirstOrDefault(i => i.InsuranceId == iId);
                if (insurance == null) throw new NotFoundExpection("Insurance", iId);
                
                if (!insurance.IsActive && !currentInsuranceIds.Contains(iId)) 
                    throw new BadRequestException($"التأمين '{insurance.Name}' معطل ولا يمكن إضافته حالياً.");
                
                existing.ProjectInsurance.Add(new ProjectInsurance { InsuranceId = iId });
            }
        }

        // Recalculate Project Stats & Status
        if (projectWithUnits != null)
        {
            var allUnits = projectWithUnits.Buildings.SelectMany(b => b.Units).ToList();

            // 1. Update each building's derived status
            foreach (var b in projectWithUnits.Buildings)
            {
                b.Status = ProjectLogicHelpers.DeriveBuildingStatus(b.Type, b.Units);
                b.FloorCount = b.Units.Any() ? b.Units.Max(u => u.Floor) : 0;
                unitOfWork.Repository<Building>().Update(b);
            }

            // 2. Update project status
            var oldStatusValue = existing.Status;
            existing.Status = ProjectLogicHelpers.DeriveProjectStatus(allUnits, existing.Status);

            existing.AvailableUnitsCount  = allUnits.Count(u => u.Status == UnitStatus.Sale   || u.Status == UnitStatus.Rent);
            existing.TransactedUnitsCount = allUnits.Count(u => u.Status == UnitStatus.Sold   || u.Status == UnitStatus.Rented);
            existing.TotalBuildingArea    = projectWithUnits.Buildings.Sum(b => b.BuildingArea);
        }


        unitOfWork.Repository<Project>().Update(existing);

        var publicMediaToDelete = new List<(string PublicId, MediaType Type)>();

        // Process Media deletions during update
        if (projectDto.DeletedMediaIds != null && projectDto.DeletedMediaIds.Any())
        {
            var mediaToDelete = existing.Media.Where(m => projectDto.DeletedMediaIds.Contains(m.MediaId)).ToList();
            var mediaRepo = unitOfWork.Repository<ProjectMedia>();
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
            if (projectDto.ThumbnailImage != null)
            {
                var oldThumb = existing.Media.FirstOrDefault(media => media.IsThumbnail);
                if (oldThumb != null)
                {
                    if (!string.IsNullOrEmpty(oldThumb.PublicId)) publicMediaToDelete.Add((oldThumb.PublicId, oldThumb.Type));
                    unitOfWork.Repository<ProjectMedia>().Remove(oldThumb);
                    existing.Media.Remove(oldThumb);
                }
                var res = await mediaService.UploadImageAsync(projectDto.ThumbnailImage);
                newlyUploadedImageIds.Add(res.PublicId);
                existing.Media.Add(new ProjectMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Image, IsThumbnail = true });
            }

            if (projectDto.Images != null && projectDto.Images.Any())
            {
                foreach (var img in projectDto.Images)
                {
                    var res = await mediaService.UploadImageAsync(img);
                    newlyUploadedImageIds.Add(res.PublicId);
                    existing.Media.Add(new ProjectMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Image, IsThumbnail = false });
                }
            }

            if (projectDto.VideoFile != null)
            {
                var oldVideo = existing.Media.FirstOrDefault(media => media.Type == MediaType.Video);
                if (oldVideo != null)
                {
                    if (!string.IsNullOrEmpty(oldVideo.PublicId)) publicMediaToDelete.Add((oldVideo.PublicId, oldVideo.Type));
                    unitOfWork.Repository<ProjectMedia>().Remove(oldVideo);
                    existing.Media.Remove(oldVideo);
                }
                var res = await mediaService.UploadVideoAsync(projectDto.VideoFile);
                newlyUploadedVideoIds.Add(res.PublicId);
                existing.Media.Add(new ProjectMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Video, IsThumbnail = false });
            }

            if (projectDto.Panorama360 != null)
            {
                var oldPano = existing.Media.FirstOrDefault(media => media.Type == MediaType.Panorama360);
                if (oldPano != null)
                {
                    if (!string.IsNullOrEmpty(oldPano.PublicId)) publicMediaToDelete.Add((oldPano.PublicId, oldPano.Type));
                    unitOfWork.Repository<ProjectMedia>().Remove(oldPano);
                    existing.Media.Remove(oldPano);
                }
                var res = await mediaService.UploadImageAsync(projectDto.Panorama360);
                newlyUploadedImageIds.Add(res.PublicId);
                existing.Media.Add(new ProjectMedia { MediaUrl = res.Url, PublicId = res.PublicId, Type = MediaType.Panorama360, IsThumbnail = false });
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

        var updatedProject = await GetProjectByIdAsync(id);
        if (updatedProject == null)
            throw new NotFoundExpection("Project", id);

        return updatedProject;
    }

    public async Task<ProjectDetailsDto> DeleteProjectAsync(int id)
    {
        var spec = new ProjectWithBuildingsSpecification(id);
        var project = await unitOfWork.Repository<Project>().GetByIdAsync(spec);

        if (project == null)
            throw new NotFoundExpection("Project", id);

        var cloudinaryImagesToDelete = new List<string>();
        var cloudinaryVideosToDelete = new List<string>();

        // 1. Get all units and their media under this project
        var unitSpec = new UnitsByProjectSpecification(id);
        var units = await unitOfWork.Repository<Unit>().GetAllAsync(unitSpec);

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

        // 2. Remove all buildings in the project
        var buildings = await unitOfWork.Repository<Building>()
            .GetAllAsync(new BuildingsByProjectSpecification(id));
        foreach (var building in buildings)
        {
            unitOfWork.Repository<Building>().Remove(building);
        }

        // 3. Collect project's own media
        foreach (var m in project.Media.ToList())
        {
            if (!string.IsNullOrEmpty(m.PublicId))
            {
                if (m.Type == MediaType.Video) cloudinaryVideosToDelete.Add(m.PublicId);
                else cloudinaryImagesToDelete.Add(m.PublicId);
            }
            unitOfWork.Repository<ProjectMedia>().Remove(m);
        }

        // 4. Remove project itself
        unitOfWork.Repository<Project>().Remove(project);

        // 5. Complete Database Transaction
        await unitOfWork.CompleteAsync();

        // 6. Clean up Cloudinary after successful DB commit
        foreach (var pid in cloudinaryImagesToDelete) await mediaService.DeleteImageAsync(pid);
        foreach (var pid in cloudinaryVideosToDelete) await mediaService.DeleteVideoAsync(pid);

        return mapper.Map<ProjectDetailsDto>(project);
    }

    private async Task ProcessFeaturesAndInsurancesAsync(Project project, List<int> featureIds, List<int> insuranceIds)
    {
        project.ProjectFeatures.Clear();
        if (featureIds.Any())
        {
            var features = await unitOfWork.Repository<Feature>().GetAllAsync();
            foreach (var fId in featureIds)
            {
                var feature = features.FirstOrDefault(f => f.FeatureId == fId);
                if (feature == null) throw new NotFoundExpection("Feature", fId);
                if (!feature.IsActive) throw new BadRequestException($"الميزة '{feature.Name}' معطلة.");
                project.ProjectFeatures.Add(new ProjectFeature { FeatureId = fId });
            }
        }

        project.ProjectInsurance.Clear();
        if (insuranceIds.Any())
        {
            var insurances = await unitOfWork.Repository<Insurance>().GetAllAsync();
            foreach (var iId in insuranceIds)
            {
                var insurance = insurances.FirstOrDefault(i => i.InsuranceId == iId);
                if (insurance == null) throw new NotFoundExpection("Insurance", iId);
                if (!insurance.IsActive) throw new BadRequestException($"التأمين '{insurance.Name}' معطل.");
                project.ProjectInsurance.Add(new ProjectInsurance { InsuranceId = iId });
            }
        }
    }


    private void ValidateMediaFiles(Microsoft.AspNetCore.Http.IFormFile? thumbnail, List<Microsoft.AspNetCore.Http.IFormFile>? images, Microsoft.AspNetCore.Http.IFormFile? panorama, Microsoft.AspNetCore.Http.IFormFile? video)
    {
        var validImageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        var validVideoTypes = new[] { "video/mp4", "video/avi", "video/mpeg", "video/quicktime" };

        if (thumbnail != null && !validImageTypes.Contains(thumbnail.ContentType.ToLower()))
            throw new BadRequestException("Thumbnail must be a valid image format (JPEG, PNG, GIF, WEBP).");

        if (images != null)
            foreach (var img in images)
                if (!validImageTypes.Contains(img.ContentType.ToLower()))
                    throw new BadRequestException("One or more images have an invalid format. Only images are allowed.");

        if (panorama != null && !validImageTypes.Contains(panorama.ContentType.ToLower()))
            throw new BadRequestException("Panorama must be a valid image format.");

        if (video != null && !validVideoTypes.Contains(video.ContentType.ToLower()))
            throw new BadRequestException("Video must be a valid video format (MP4, AVI, MPEG, MOV).");
    }
}
