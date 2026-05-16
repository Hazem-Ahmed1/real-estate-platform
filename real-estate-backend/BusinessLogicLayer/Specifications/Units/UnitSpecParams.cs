
namespace BusinessLogicLayer.Specifications.Units;

public class UnitSpecParams
{
    private const int MaxPageSize = 50;
    public int Page { get; set; } = 1;
    private int _pageSize = 12;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    public UnitType? Type { get; set; }
    public UnitStatus? Status { get; set; }
    public int? Rooms { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Search { get; set; }
    public int? BuildingId { get; set; }
    public int? ProjectId { get; set; }
    public bool PublicOnly { get; set; }
}
