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

    public async Task<ProjectDetailsDto> CreateProjectAsync(ProjectDto projectDto)
    {
        if (projectDto.BuildUpArea.HasValue && projectDto.LandArea.HasValue && projectDto.BuildUpArea > projectDto.LandArea)
            throw new BadRequestException("BuildUpArea must be less than or equal to LandArea.");

        if (projectDto.TotalBuildingArea.HasValue && projectDto.BuildUpArea.HasValue && projectDto.TotalBuildingArea > projectDto.BuildUpArea)
            throw new BadRequestException("TotalBuildingArea must be less than or equal to BuildUpArea.");

        var exists = await unitOfWork.Repository<Project>()
            .AnyAsync(p => p.Name == projectDto.Name);

        if (exists)
            throw new ConflictException("Project with the same name already exists.");

        var project = mapper.Map<Project>(projectDto);
        project.IsStatusChanged = false; // Cannot be set during create
        // Initial status is provided in the DTO (Sale or Rent)



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

        await unitOfWork.Repository<Project>().AddAsync(project);
        await unitOfWork.CompleteAsync();

        // Process Media
        if (projectDto.ThumbnailImage != null)
        {
            var url = await mediaService.UploadImageAsync(projectDto.ThumbnailImage);
            var media = new ProjectMedia { ProjectId = project.ProjectId, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Image, IsThumbnail = true };
            await unitOfWork.Repository<ProjectMedia>().AddAsync(media);
        }

        if (projectDto.Images != null && projectDto.Images.Any())
        {
            foreach (var img in projectDto.Images)
            {
                var url = await mediaService.UploadImageAsync(img);
                var media = new ProjectMedia { ProjectId = project.ProjectId, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Image, IsThumbnail = false };
                await unitOfWork.Repository<ProjectMedia>().AddAsync(media);
            }
        }

        if (projectDto.Video != null)
        {
            var url = await mediaService.UploadImageAsync(projectDto.Video);
            var media = new ProjectMedia { ProjectId = project.ProjectId, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Video, IsThumbnail = false };
            await unitOfWork.Repository<ProjectMedia>().AddAsync(media);
        }

        if (projectDto.Panorama360 != null)
        {
            var url = await mediaService.UploadImageAsync(projectDto.Panorama360);
            var media = new ProjectMedia { ProjectId = project.ProjectId, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Panorama360, IsThumbnail = false };
            await unitOfWork.Repository<ProjectMedia>().AddAsync(media);
        }

        await unitOfWork.CompleteAsync();

        var created = await GetProjectByIdAsync(project.ProjectId);
        if (created == null)
            throw new NotFoundExpection("Project", project.ProjectId);

        return created;
    }

    public async Task<ProjectDetailsDto> UpdateProjectAsync(int id, ProjectDto projectDto)
    {
        if (projectDto.BuildUpArea.HasValue && projectDto.LandArea.HasValue && projectDto.BuildUpArea > projectDto.LandArea)
            throw new BadRequestException("BuildUpArea must be less than or equal to LandArea.");

        if (projectDto.TotalBuildingArea.HasValue && projectDto.BuildUpArea.HasValue && projectDto.TotalBuildingArea > projectDto.BuildUpArea)
            throw new BadRequestException("TotalBuildingArea must be less than or equal to BuildUpArea.");

        var spec = new ProjectWithBuildingsSpecification(id);
        var existing = await unitOfWork.Repository<Project>().GetByIdAsync(spec);

        if (existing == null)
            throw new NotFoundExpection("Project", id);

        var exists = await unitOfWork.Repository<Project>()
            .AnyAsync(p => p.Name == projectDto.Name && p.ProjectId != id);

        if (exists)
            throw new ConflictException("Project with the same name already exists.");

        var oldStatus = existing.Status;
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


            // Area Calculations
            // TotalBuildingArea = Σ (BuildingArea_i) + Σ (Max unit area in Building_i)
            var projectBuildings = projectWithUnits.Buildings.ToList();

            existing.TotalBuildingArea = projectBuildings.Sum(b => (b.BuildingArea ?? 0) + (b.Units.Any() ? b.Units.Max(u => u.Area ?? 0) : 0));
        }


        unitOfWork.Repository<Project>().Update(existing);

        // Process Media deletions during update
        if (projectDto.DeletedMediaIds != null && projectDto.DeletedMediaIds.Any())
        {
            var mediaToDelete = existing.Media.Where(m => projectDto.DeletedMediaIds.Contains(m.MediaId)).ToList();
            foreach (var m in mediaToDelete)
            {
                if (!string.IsNullOrEmpty(m.PublicId))
                    await mediaService.DeleteImageAsync(m.PublicId);
                unitOfWork.Repository<ProjectMedia>().Remove(m);
            }
        }

        // Replace Thumbnail
        if (projectDto.ThumbnailImage != null)
        {
            var oldThumb = existing.Media.FirstOrDefault(m => m.IsThumbnail);
            if (oldThumb != null)
            {
                if (!string.IsNullOrEmpty(oldThumb.PublicId)) await mediaService.DeleteImageAsync(oldThumb.PublicId);
                unitOfWork.Repository<ProjectMedia>().Remove(oldThumb);
            }

            var url = await mediaService.UploadImageAsync(projectDto.ThumbnailImage);
            var media = new ProjectMedia { ProjectId = id, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Image, IsThumbnail = true };
            await unitOfWork.Repository<ProjectMedia>().AddAsync(media);
        }

        // Append new images
        if (projectDto.Images != null && projectDto.Images.Any())
        {
            foreach (var img in projectDto.Images)
            {
                var url = await mediaService.UploadImageAsync(img);
                var media = new ProjectMedia { ProjectId = id, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Image, IsThumbnail = false };
                await unitOfWork.Repository<ProjectMedia>().AddAsync(media);
            }
        }

        // Replace Video
        if (projectDto.Video != null)
        {
            var oldVideo = existing.Media.FirstOrDefault(m => m.Type == MediaType.Video);
            if (oldVideo != null)
            {
                if (!string.IsNullOrEmpty(oldVideo.PublicId)) await mediaService.DeleteImageAsync(oldVideo.PublicId);
                unitOfWork.Repository<ProjectMedia>().Remove(oldVideo);
            }

            var url = await mediaService.UploadImageAsync(projectDto.Video);
            var media = new ProjectMedia { ProjectId = id, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Video, IsThumbnail = false };
            await unitOfWork.Repository<ProjectMedia>().AddAsync(media);
        }

        // Replace Panorama
        if (projectDto.Panorama360 != null)
        {
            var oldPano = existing.Media.FirstOrDefault(m => m.Type == MediaType.Panorama360);
            if (oldPano != null)
            {
                if (!string.IsNullOrEmpty(oldPano.PublicId)) await mediaService.DeleteImageAsync(oldPano.PublicId);
                unitOfWork.Repository<ProjectMedia>().Remove(oldPano);
            }

            var url = await mediaService.UploadImageAsync(projectDto.Panorama360);
            var media = new ProjectMedia { ProjectId = id, MediaUrl = url.Url, PublicId = url.PublicId, Type = MediaType.Panorama360, IsThumbnail = false };
            await unitOfWork.Repository<ProjectMedia>().AddAsync(media);
        }

        await unitOfWork.CompleteAsync();

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


        foreach (var m in project.Media)
        {
            if (!string.IsNullOrEmpty(m.PublicId))
            {
                await mediaService.DeleteImageAsync(m.PublicId);
            }
        }
        unitOfWork.Repository<Project>().Remove(project);
        await unitOfWork.CompleteAsync();

        return mapper.Map<ProjectDetailsDto>(project);
    }


    public async Task<List<string>> GetAvailableCitiesAsync()
    {
        var projects = await unitOfWork.Repository<Project>().GetAllAsync(asNoTracking: true);
        return projects
            .Select(p => p.City)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToList()!;
    }

    private static ProjectStatus DeriveProjectStatus(IEnumerable<DataAccessLayer.Entities.UnitModule.Unit> units, ProjectStatus currentStatus)
    {
        return BusinessLogicLayer.Helpers.ProjectLogicHelpers.DeriveProjectStatus(units, currentStatus);
    }

}
