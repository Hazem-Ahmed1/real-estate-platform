namespace DataAccessLayer.Common;

public record PaginatedResult<TData>
{
    public PaginatedResult(int page, int pageSize, int totalItems, IEnumerable<TData> items)
    {
        Page = page;
        PageSize = pageSize;
        TotalItems = totalItems;
        Items = items;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    }

    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public IEnumerable<TData> Items { get; init; }
}
