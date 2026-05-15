using BusinessLogicLayer.Specifications.Units;

namespace BusinessLogicLayer.Specifications.Units;

public class UnitWithDetailsSpecification : BaseSpecifications<Unit>
{
    public UnitWithDetailsSpecification(UnitSpecParams @params)
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
        AddInclude(x => x.Media);
        AddInclude(x => x.NearbyFacilities);

        if (!@params.DisablePaging)
            ApplyPagination(@params.PageSize, @params.Page);

        if (!string.IsNullOrEmpty(@params.Sort))
        {
            switch (@params.Sort)
            {
                case "priceAsc":
                    AddOrderBy(p => p.Price ?? 0);
                    break;
                case "priceDesc":
                    AddOrderByDescending(p => p.Price ?? 0);
                    break;
                case "areaAsc":
                    AddOrderBy(p => p.Area ?? 0);
                    break;
                case "areaDesc":
                    AddOrderByDescending(p => p.Area ?? 0);
                    break;
                default:
                    AddOrderByDescending(p => p.CreatedAt);
                    break;
            }
        }
        else
        {
            AddOrderByDescending(p => p.CreatedAt);
        }
    }

    public UnitWithDetailsSpecification(int id) : base(x => x.UnitId == id)
    {
        AddInclude(x => x.Building);
        AddInclude("Building.Project");
        AddInclude(x => x.Media);
        AddInclude(x => x.NearbyFacilities);
    }
}
