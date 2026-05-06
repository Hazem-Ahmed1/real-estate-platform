using BusinessLogicLayer.Dtos.BlogModule;

namespace BusinessLogicLayer.Contracts;

public interface IBlogService
{
    Task<IReadOnlyList<BlogListDto>> GetBlogsAsync();
    Task<BlogDetailsDto> CreateBlogAsync(BlogCreateDto dto);
}
