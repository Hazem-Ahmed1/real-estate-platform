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

    public async Task<ProjectDetailsDto?> GetProjectByIdAsync(int id, bool publicOnly = false)
    {
        var spec = new ProjectWithBuildingsSpecification(id);
        var project = await unitOfWork.Repository<Project>().GetByIdAsync(spec);

        if (project == null)
            return null;


        return mapper.Map<ProjectDetailsDto>(project);
    }

    public async Task<ProjectDetailsDto> CreateProjectAsync(ProjectCreateDto projectDto)
    {
        ValidateMediaFiles(projectDto.ThumbnailImage, projectDto.Images, projectDto.Panorama360, projectDto.VideoFile);

        // 1. Initial Validation
        if (projectDto.BuildUpArea > projectDto.LandArea)
            throw new BadRequestException("Build-up area must be less than or equal to Land area.");

        if (projectDto.TotalBuildingArea.HasValue && projectDto.TotalBuildingArea > projectDto.BuildUpArea)
            throw new BadRequestException("Total building area must be less than or equal to Build-up area.");

        var exists = await unitOfWork.Repository<Project>()
            .AnyAsync(p => p.Name == projectDto.Name);

        if (exists)
            throw new ConflictException("Project with the same name already exists.");

        var project = mapper.Map<Project>(projectDto);
        project.IsStatusChanged = false;

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

        if (oldStatus != newStatus)
        {
            // 1. Terminal Check: Sold cannot go back to Sale
            if (oldStatus == ProjectStatus.Sold && newStatus == ProjectStatus.Sale)
            {
                throw new BadRequestException("لا يمكن تحويل المشروع من حالة 'مباع بالكامل' (Sold) إلى 'للبيع' (Sale) مرة أخرى.");
            }

            // 2. Path Separation: No crossing between Sale and Rent paths
            if ((oldStatus == ProjectStatus.Sale || oldStatus == ProjectStatus.Sold) && 
                (newStatus == ProjectStatus.Rent || newStatus == ProjectStatus.Rented))
            {
                throw new BadRequestException($"لا يمكن تحويل المشروع من مسار البيع ({oldStatus}) إلى مسار الإيجار ({newStatus}).");
            }

            if ((oldStatus == ProjectStatus.Rent || oldStatus == ProjectStatus.Rented) && 
                (newStatus == ProjectStatus.Sale || newStatus == ProjectStatus.Sold))
            {
                throw new BadRequestException($"لا يمكن تحويل المشروع من مسار الإيجار ({oldStatus}) إلى مسار البيع ({newStatus}).");
            }
        }

        mapper.Map(projectDto, existing);


        if (oldStatus != existing.Status)
            existing.IsStatusChanged = true;

        // Ensure explicit override from DTO if provided (since it's only modifiable in Edit)
        if (projectDto.IsStatusChanged)
            existing.IsStatusChanged = true;
        else if (existing.IsStatusChanged && !projectDto.IsStatusChanged)
            existing.IsStatusChanged = false; // User can manually reset it


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
        var projectWithUnits = await unitOfWork.Repository<Project>().GetByIdAsync(new ProjectWithBuildingsSpecification(id));
        if (projectWithUnits != null)
        {
            var allUnits = projectWithUnits.Buildings.SelectMany(b => b.Units).ToList();
            var statusBefore = existing.Status;
            existing.Status = DeriveProjectStatus(allUnits, existing.Status);

            if (statusBefore != existing.Status)
                existing.IsStatusChanged = true;

            bool isSale = existing.Status == ProjectStatus.Sale || existing.Status == ProjectStatus.Sold;

            if (isSale)
            {
                existing.AvailableUnitsCount = allUnits.Count(u => u.Status == UnitStatus.Sale);
                existing.TransactedUnitsCount = allUnits.Count(u => u.Status == UnitStatus.Sold);
            }
            else
            {
                existing.AvailableUnitsCount = allUnits.Count(u => u.Status == UnitStatus.Rent);
                existing.TransactedUnitsCount = allUnits.Count(u => u.Status == UnitStatus.Rented);
            }


            // Recalculate Building FloorCounts and Project Area/Status
            var projectBuildings = projectWithUnits.Buildings.ToList();
            foreach (var b in projectBuildings)
            {
                b.FloorCount = b.Units.Any() ? b.Units.Max(u => u.Floor) : 0;
                unitOfWork.Repository<Building>().Update(b);
            }

            // TotalBuildingArea = Σ (BuildingArea_i) + Σ (Max unit area in Building_i)
            existing.TotalBuildingArea = projectBuildings.Sum(b => b.BuildingArea + (b.Units.Any() ? b.Units.Max(u => u.Area) : 0));
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

        if (project.Status == ProjectStatus.Sold || project.Status == ProjectStatus.Rented)
            throw new BadRequestException("Cannot delete a project that has reached Sold or Rented status.");

        var hasBuildings = await unitOfWork.Repository<Building>()
            .AnyAsync(b => b.ProjectId == id);
            
        if (hasBuildings)
            throw new BadRequestException("Cannot delete a project that still has buildings. Delete the buildings first.");


        var publicMediaToDelete = new List<(string PublicId, MediaType Type)>();

        if (project.Media.Count > 0)
        {
            var mediaRepo = unitOfWork.Repository<ProjectMedia>();
            foreach (var m in project.Media.ToList())
            {
                if (!string.IsNullOrEmpty(m.PublicId))
                {
                    publicMediaToDelete.Add((m.PublicId, m.Type));
                }
                mediaRepo.Remove(m);
            }
        }
        unitOfWork.Repository<Project>().Remove(project);
        await unitOfWork.CompleteAsync();

        foreach (var m in publicMediaToDelete)
        {
            if (m.Type == MediaType.Video)
                await mediaService.DeleteVideoAsync(m.PublicId);
            else
                await mediaService.DeleteImageAsync(m.PublicId);
        }

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

    private static ProjectStatus DeriveProjectStatus(IEnumerable<DataAccessLayer.Entities.UnitModule.Unit> units, ProjectStatus currentStatus)
    {
        return BusinessLogicLayer.Helpers.ProjectLogicHelpers.DeriveProjectStatus(units, currentStatus);
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
