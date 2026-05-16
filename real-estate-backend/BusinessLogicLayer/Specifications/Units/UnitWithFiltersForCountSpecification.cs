using BusinessLogicLayer.Specifications.Units;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Enums;

namespace BusinessLogicLayer.Specifications.Units;

public class UnitWithFiltersForCountSpecification : BaseSpecifications<Unit>
{
    public UnitWithFiltersForCountSpecification(UnitSpecParams @params)
        : base(x =>
            (!@params.PublicOnly || (x.Status == UnitStatus.Sale || x.Status == UnitStatus.Rent)) &&
            (string.IsNullOrEmpty(@params.Search) || (x.Street != null && x.Street.Contains(@params.Search))) &&
            (!@params.Type.HasValue || x.Type == @params.Type) &&
            (!@params.Status.HasValue || x.Status == @params.Status) &&
            (!@params.Rooms.HasValue || x.Rooms == @params.Rooms) &&
            (!@params.MinPrice.HasValue || x.Price >= @params.MinPrice) &&
            (!@params.MaxPrice.HasValue || x.Price <= @params.MaxPrice) &&
            (!@params.BuildingId.HasValue || x.BuildingId == @params.BuildingId) &&
            (!@params.ProjectId.HasValue || x.Building.ProjectId == @params.ProjectId)

        )
    {
        AddInclude(x => x.Building);
        AddInclude("Building.Project");
    }

}
