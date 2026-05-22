using BusinessLogicLayer.Specifications.Units;
using DataAccessLayer.Enums;

namespace BusinessLogicLayer.Specifications.Units;

public class UnitWithDetailsSpecification : BaseSpecifications<Unit>
{
    public UnitWithDetailsSpecification(UnitSpecParams @params)
        : base(x =>
            (string.IsNullOrEmpty(@params.Search) || (x.Address != null && x.Address.Contains(@params.Search))) &&
            (string.IsNullOrEmpty(@params.City) || (x.Building.Project.City != null && x.Building.Project.City.ToLower() == @params.City.ToLower())) &&
            (!@params.Type.HasValue || x.Type == @params.Type) &&
            (@params.IncludeAllStatuses || (@params.Status.HasValue ? x.Status == @params.Status : (x.Status == UnitStatus.Sale || x.Status == UnitStatus.Rent))) &&
            (!@params.Rooms.HasValue || x.Rooms == @params.Rooms) &&
            (!@params.MinPrice.HasValue || x.Price >= @params.MinPrice) &&
            (!@params.MaxPrice.HasValue || x.Price <= @params.MaxPrice) &&
            (!@params.BuildingId.HasValue || x.BuildingId == @params.BuildingId) &&
            (!@params.ProjectId.HasValue || x.Building.ProjectId == @params.ProjectId)
        )
    {
        AddInclude(x => x.Building);
        AddInclude("Building.Project");
        AddInclude(x => x.Media);
        AddInclude(x => x.NearbyFacilities);

        ApplyPagination(@params.PageSize, @params.Page);

        AddOrderByDescending(p => p.CreatedAt);
    }

    public UnitWithDetailsSpecification(int id) : base(x => x.UnitId == id)
    {
        AddInclude(x => x.Building);
        AddInclude("Building.Project");
        AddInclude(x => x.Media);
        AddInclude(x => x.NearbyFacilities);
        AddInclude("UnitFeatures.Feature");
        AddInclude("UnitInsurance.Insurance");
    }
}
