using BusinessLogicLayer.Specifications.Units;

namespace BusinessLogicLayer.Specifications.Units;

public class UnitWithFiltersForCountSpecification : BaseSpecifications<Unit>
{
    public UnitWithFiltersForCountSpecification(UnitSpecParams @params)
        : base(x =>
            (!@params.PublicOnly || (x.Status == UnitStatus.Sale || x.Status == UnitStatus.Rent)) &&
            (string.IsNullOrEmpty(@params.Search) || (x.Street != null && x.Street.Contains(@params.Search))) &&
            (string.IsNullOrEmpty(@params.City) || (x.Building.Project.City != null && x.Building.Project.City == @params.City)) &&
            (!@params.Type.HasValue || x.Type == @params.Type) &&
            (!@params.Status.HasValue || x.Status == @params.Status) &&
            (!@params.Rooms.HasValue || x.Rooms == @params.Rooms) &&
            (!@params.MinPrice.HasValue || x.Price >= @params.MinPrice) &&
            (!@params.MaxPrice.HasValue || x.Price <= @params.MaxPrice) &&
            (!@params.BuildingId.HasValue || x.BuildingId == @params.BuildingId)

        )
    {
        AddInclude(x => x.Building);
        AddInclude("Building.Project");
    }

}
