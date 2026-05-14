using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.BlogModule;
using BusinessLogicLayer.Specifications.Blogs;
using DataAccessLayer.Common;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers;

public class BlogsController(IBlogService blogService) : ApiController
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<BlogListDto>>> GetBlogs([FromQuery] BlogSpecParams @params)
    {
        var result = await blogService.GetBlogsAsync(@params);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BlogDetailsDto>> GetBlog(int id)
    {
        var blog = await blogService.GetBlogByIdAsync(id);
        return Ok(blog);
    }

}
