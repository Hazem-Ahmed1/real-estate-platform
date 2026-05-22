using BusinessLogicLayer.Common;

namespace BusinessLogicLayer.Implementation;

public class LookupService(IUnitOfWork unitOfWork, IMapper mapper) : ILookupService
{
    #region Features
    public async Task<IReadOnlyList<FeatureDto>> GetFeaturesAsync(LookupStatus status = LookupStatus.Active)
    {
        var features = (await unitOfWork.Repository<Feature>().GetAllAsync()).Cast<Feature>().ToList();
        var filtered = ApplyStatusFilter(features, status);
        
        var dtos = new List<FeatureDto>();
        foreach (var feature in filtered)
        {
            var dto = mapper.Map<FeatureDto>(feature);
            dto.ProjectCount = (await unitOfWork.Repository<ProjectFeature>().GetAllAsync(pf => pf.FeatureId == feature.FeatureId)).Count();
            dto.UnitCount    = (await unitOfWork.Repository<UnitFeature>().GetAllAsync(uf => uf.FeatureId == feature.FeatureId)).Count();
            dtos.Add(dto);
        }
        return dtos.AsReadOnly();
    }

    public async Task<FeatureDto> GetFeatureByIdAsync(int id)
    {
        var feature = await unitOfWork.Repository<Feature>().GetByIdAsync(id);
        if (feature == null)
            throw new NotFoundExpection("Feature", id);
        return mapper.Map<FeatureDto>(feature);
    }

    public async Task<FeatureDto> CreateFeatureAsync(LookupUpsertDto dto)
    {
        var exists = await unitOfWork.Repository<Feature>()
            .AnyAsync(f => f.Name == dto.Name);

        if (exists)
            throw new ConflictException("هذا الاسم موجود مسبقاً، إذا كان معطلاً يمكنك تفعيله من قائمة العناصر المعطلة");

        var feature = new Feature
        {
            Name = dto.Name,
            IsActive = true
        };

        await unitOfWork.Repository<Feature>().AddAsync(feature);
        await unitOfWork.CompleteAsync();

        return mapper.Map<FeatureDto>(feature);
    }

    public async Task<FeatureDto> UpdateFeatureAsync(int id, UpdateFeatureDto dto)
    {
        var existing = await unitOfWork.Repository<Feature>().GetByIdAsync(id);

        if (existing == null)
            throw new NotFoundExpection("Feature", id);

        var exists = await unitOfWork.Repository<Feature>()
            .AnyAsync(f => f.Name == dto.Name && f.FeatureId != id);

        if (exists)
            throw new ConflictException("هذا الاسم موجود مسبقاً");

        existing.Name = dto.Name;
        existing.IsActive = dto.IsActive;

        unitOfWork.Repository<Feature>().Update(existing);
        await unitOfWork.CompleteAsync();

        return mapper.Map<FeatureDto>(existing);
    }

    public async Task<LookupDeleteResultDto<FeatureDto>> DeleteFeatureAsync(int id)
    {
        var feature = await unitOfWork.Repository<Feature>().GetByIdAsync(id);

        if (feature == null)
            throw new NotFoundExpection("Feature", id);

        var isUsedInProjects = await unitOfWork.Repository<ProjectFeature>()
            .AnyAsync(pf => pf.FeatureId == id);

        var isUsedInUnits = await unitOfWork.Repository<UnitFeature>()
            .AnyAsync(uf => uf.FeatureId == id);

        if (isUsedInProjects || isUsedInUnits)
        {
            feature.IsActive = false;
            unitOfWork.Repository<Feature>().Update(feature);
        }
        else
        {
            unitOfWork.Repository<Feature>().Remove(feature);
        }

        await unitOfWork.CompleteAsync();

        return new LookupDeleteResultDto<FeatureDto>
        {
            Message = isUsedInProjects || isUsedInUnits
                ? "تم تعطيل الميزة لأنها مستخدمة في مشاريع أو وحدات"
                : "تم حذف الميزة نهائياً",
            Item = mapper.Map<FeatureDto>(feature)
        };
    }

    #endregion

    #region Insurance

    public async Task<IReadOnlyList<InsuranceDto>> GetInsurancesAsync(LookupStatus status = LookupStatus.Active)
    {
        var insurances = (await unitOfWork.Repository<Insurance>().GetAllAsync()).Cast<Insurance>().ToList();
        var filtered = ApplyStatusFilter(insurances, status);

        var dtos = new List<InsuranceDto>();
        foreach (var insurance in filtered)
        {
            var dto = mapper.Map<InsuranceDto>(insurance);
            dto.ProjectCount = (await unitOfWork.Repository<ProjectInsurance>().GetAllAsync(pi => pi.InsuranceId == insurance.InsuranceId)).Count();
            dto.UnitCount    = (await unitOfWork.Repository<UnitInsurance>().GetAllAsync(ui => ui.InsuranceId == insurance.InsuranceId)).Count();
            dtos.Add(dto);
        }
        return dtos.AsReadOnly();
    }

    public async Task<InsuranceDto> GetInsuranceByIdAsync(int id)
    {
        var insurance = await unitOfWork.Repository<Insurance>().GetByIdAsync(id);
        if (insurance == null)
            throw new NotFoundExpection("Insurance", id);
        return mapper.Map<InsuranceDto>(insurance);
    }

    public async Task<InsuranceDto> CreateInsuranceAsync(LookupUpsertDto dto)
    {
        var exists = await unitOfWork.Repository<Insurance>()
            .AnyAsync(i => i.Name == dto.Name);

        if (exists)
            throw new ConflictException("هذا الاسم موجود مسبقاً، إذا كان معطلاً يمكنك تفعيله من قائمة العناصر المعطلة");

        var insurance = new Insurance
        {
            Name = dto.Name,
            Duration = dto.Duration ?? 1,
            IsActive = true
        };

        await unitOfWork.Repository<Insurance>().AddAsync(insurance);
        await unitOfWork.CompleteAsync();

        return mapper.Map<InsuranceDto>(insurance);
    }

    public async Task<InsuranceDto> UpdateInsuranceAsync(int id, UpdateInsuranceDto dto)
    {
        var existing = await unitOfWork.Repository<Insurance>().GetByIdAsync(id);

        if (existing == null)
            throw new NotFoundExpection("Insurance", id);

        var exists = await unitOfWork.Repository<Insurance>()
            .AnyAsync(i => i.Name == dto.Name && i.InsuranceId != id);

        if (exists)
            throw new ConflictException("هذا الاسم موجود مسبقاً");

        existing.Name = dto.Name;
        existing.Duration = dto.Duration;
        existing.IsActive = dto.IsActive;

        unitOfWork.Repository<Insurance>().Update(existing);
        await unitOfWork.CompleteAsync();

        return mapper.Map<InsuranceDto>(existing);
    }

    public async Task<LookupDeleteResultDto<InsuranceDto>> DeleteInsuranceAsync(int id)
    {
        var insurance = await unitOfWork.Repository<Insurance>().GetByIdAsync(id);

        if (insurance == null)
            throw new NotFoundExpection("Insurance", id);

        var isUsedInProjects = await unitOfWork.Repository<ProjectInsurance>()
            .AnyAsync(pi => pi.InsuranceId == id);

        var isUsedInUnits = await unitOfWork.Repository<UnitInsurance>()
            .AnyAsync(ui => ui.InsuranceId == id);

        var isUsed = isUsedInProjects || isUsedInUnits;

        if (isUsed)
        {
            insurance.IsActive = false;
            unitOfWork.Repository<Insurance>().Update(insurance);
        }
        else
        {
            unitOfWork.Repository<Insurance>().Remove(insurance);
        }

        await unitOfWork.CompleteAsync();

        return new LookupDeleteResultDto<InsuranceDto>
        {
            Message = isUsed
                ? "تم تعطيل التأمين لأنه مستخدم في مشاريع أو وحدات"
                : "تم حذف التأمين نهائياً",
            Item = mapper.Map<InsuranceDto>(insurance)
        };
    }

    #endregion

    private static IEnumerable<T> ApplyStatusFilter<T>(IEnumerable<T> items, LookupStatus status) where T : class
    {
        return status switch
        {
            LookupStatus.Active   => items.Where(x => (bool)x.GetType().GetProperty("IsActive")!.GetValue(x)!),
            LookupStatus.Inactive => items.Where(x => !(bool)x.GetType().GetProperty("IsActive")!.GetValue(x)!),
            _                     => items
        };
    }
}
