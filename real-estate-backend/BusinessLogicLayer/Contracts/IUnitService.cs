using BusinessLogicLayer.Specifications.Units;

namespace BusinessLogicLayer.Contracts;

public interface IUnitService
{
    // Public Endpoints
    Task<PaginatedResult<UnitListDto>> GetUnitsAsync(UnitSpecParams @params);
    Task<PaginatedResult<UnitListDto>> GetAdminUnitsAsync(UnitSpecParams @params);
    Task<GetUnitDto?> GetUnitByIdAsync(int id);

    // Admin Endpoints
    Task<GetUnitDto> CreateUnitAsync(UnitCreateDto unitDto);
    Task<GetUnitDto> UpdateUnitAsync(int id, UnitUpdateDto unitDto);
    Task<GetUnitDto> DeleteUnitAsync(int id);

    // Building & Project Units
    Task<IReadOnlyList<UnitListDto>> GetBuildingUnitsAsync(int buildingId);
    Task<IReadOnlyList<UnitListDto>> GetProjectUnitsAsync(int projectId);
}



