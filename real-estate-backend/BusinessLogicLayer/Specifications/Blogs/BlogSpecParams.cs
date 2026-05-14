using BusinessLogicLayer.Common;

namespace BusinessLogicLayer.Specifications.Blogs;

public class BlogSpecParams : PaginationParams
{
    // Bind from query string as `q`
    public string? Q { get; set; }
}
