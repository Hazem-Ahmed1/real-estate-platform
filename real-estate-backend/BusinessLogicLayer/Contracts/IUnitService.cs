using BusinessLogicLayer.Specifications.Units;

namespace BusinessLogicLayer.Contracts;

public interface IUnitService
{
    // Public Endpoints
    Task<PaginatedResult<UnitListDto>> GetUnitsAsync(UnitSpecParams @params);
    Task<GetUnitDto?> GetUnitByIdAsync(int id);

    // Admin Endpoints
    Task<GetUnitDto> CreateUnitAsync(CreateUnitDto unitDto);
    Task<GetUnitDto> UpdateUnitAsync(int id, UpdateUnitDto unitDto);
    Task<GetUnitDto> DeleteUnitAsync(int id);

    // Building Units
    Task<IReadOnlyList<UnitListDto>> GetBuildingUnitsAsync(int buildingId);
}



