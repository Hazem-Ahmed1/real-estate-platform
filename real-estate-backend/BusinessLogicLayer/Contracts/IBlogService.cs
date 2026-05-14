using BusinessLogicLayer.Dtos.BlogModule;
using BusinessLogicLayer.Specifications.Blogs;
using DataAccessLayer.Common;

namespace BusinessLogicLayer.Contracts;

public interface IBlogService
{
    Task<PaginatedResult<BlogListDto>> GetBlogsAsync(BlogSpecParams @params);
    Task<BlogDetailsDto> GetBlogByIdAsync(int id);
    Task<BlogDetailsDto> CreateBlogAsync(BlogCreateDto dto);
    Task<BlogDetailsDto> UpdateBlogAsync(int id, BlogUpdateDto dto);
    Task DeleteBlogAsync(int id);
}
