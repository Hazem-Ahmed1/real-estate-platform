using APILayer.Dtos.Blogs;
using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.BlogModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers;

public class BlogsController(IBlogService blogService, IMediaService mediaService) : ApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BlogListDto>>> GetBlogs()
    {
        var blogs = await blogService.GetBlogsAsync();
        return Ok(blogs);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<BlogDetailsDto>> CreateBlog([FromForm] BlogCreateFormDto form)
    {
        if (string.IsNullOrWhiteSpace(form.Title))
        {
            return BadRequest("Title is required.");
        }

        var images = new List<BlogImageCreateDto>();

        if (form.Thumbnail is not null)
        {
            var uploaded = await mediaService.UploadImageAsync(form.Thumbnail);
            images.Add(new BlogImageCreateDto(uploaded.Url, true));
        }

        if (form.Images is not null && form.Images.Count > 0)
        {
            foreach (var file in form.Images)
            {
                if (file is null || file.Length == 0)
                {
                    continue;
                }

                var uploaded = await mediaService.UploadImageAsync(file);
                images.Add(new BlogImageCreateDto(uploaded.Url, false));
            }
        }

        var dto = new BlogCreateDto(
            form.Title,
            form.Description,
            form.PublishDate,
            images.Count == 0 ? null : images
        );

        var created = await blogService.CreateBlogAsync(dto);
        return Ok(created);
    }
}
