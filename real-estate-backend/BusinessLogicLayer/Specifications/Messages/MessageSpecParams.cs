using BusinessLogicLayer.Common;

namespace BusinessLogicLayer.Specifications.Messages;

public class MessageSpecParams : PaginationParams
{
    // Bind from query string as `q`
    public string? Q { get; set; }
}
