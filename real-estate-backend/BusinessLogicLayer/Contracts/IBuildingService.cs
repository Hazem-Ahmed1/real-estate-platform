namespace BusinessLogicLayer.Contracts;

public interface IBuildingService
{
    Task<IReadOnlyList<BuildingDto>> GetAllBuildingsAsync(int? projectId = null);
    Task<BuildingDto> GetBuildingByIdAsync(int id);
    Task<IReadOnlyList<BuildingDto>> GetPublicBuildingsAsync(int? projectId = null);
    Task<BuildingDto> GetPublicBuildingByIdAsync(int id);
    Task<BuildingDto> CreateBuildingAsync(BuildingUpsertDto dto);
    Task<BuildingDto> UpdateBuildingAsync(int id, BuildingUpsertDto dto);
    Task<BuildingDto> DeleteBuildingAsync(int id);
}
