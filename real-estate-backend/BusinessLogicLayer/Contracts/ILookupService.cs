using BusinessLogicLayer.Common;

namespace BusinessLogicLayer.Contracts;

public interface ILookupService
{
    // Features
    Task<IReadOnlyList<FeatureDto>> GetFeaturesAsync(LookupStatus status = LookupStatus.Active);

    Task<FeatureDto> GetFeatureByIdAsync(int id);

    Task<FeatureDto> CreateFeatureAsync(LookupUpsertDto dto);
    Task<FeatureDto> UpdateFeatureAsync(int id, UpdateFeatureDto dto);
    Task<LookupDeleteResultDto<FeatureDto>> DeleteFeatureAsync(int id);

    // Insurance
    Task<IReadOnlyList<InsuranceDto>> GetInsurancesAsync(LookupStatus status = LookupStatus.Active);

    Task<InsuranceDto> GetInsuranceByIdAsync(int id);

    Task<InsuranceDto> CreateInsuranceAsync(LookupUpsertDto dto);
    Task<InsuranceDto> UpdateInsuranceAsync(int id, UpdateInsuranceDto dto);
    Task<LookupDeleteResultDto<InsuranceDto>> DeleteInsuranceAsync(int id);
}
