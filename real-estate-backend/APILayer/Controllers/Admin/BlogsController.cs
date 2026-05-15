using APILayer.Dtos.Blogs;
using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.BlogModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers.Admin;

[ApiController]
[Route("api/blogs")]
[Authorize(Roles = "Admin")]
public class BlogsController(IBlogService blogService, IMediaService mediaService) : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<BlogDetailsDto>> CreateBlog([FromForm] BlogCreateFormDto form)
    {
        if (string.IsNullOrWhiteSpace(form.Title))
        {
            return BadRequest("Title is required.");
        }

        var images = await UploadImagesAsync(form.Thumbnail, form.Images);

        var dto = new BlogCreateDto(
            form.Title,
            form.Description,
            form.PublishDate,
            images.Count == 0 ? null : images
        );

        var created = await blogService.CreateBlogAsync(dto);
        return Ok(created);
    }

    [HttpPut("{blogId:int}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<BlogDetailsDto>> UpdateBlog(int blogId, [FromForm] BlogUpdateFormDto form)
    {
        if (string.IsNullOrWhiteSpace(form.Title) &&
            form.Description is null &&
            !form.PublishDate.HasValue &&
            form.Thumbnail is null &&
            (form.Images is null || form.Images.Count == 0))
        {
            return BadRequest("At least one field is required.");
        }

        var images = await UploadImagesAsync(form.Thumbnail, form.Images);
        var imagesToSend = images.Count == 0 ? null : images;

        var dto = new BlogUpdateDto(
            form.Title,
            form.Description,
            form.PublishDate,
            imagesToSend
        );

        var updated = await blogService.UpdateBlogAsync(blogId, dto);
        return Ok(updated);
    }

    [HttpDelete("{blogId:int}")]
    public async Task<IActionResult> DeleteBlog(int blogId)
    {
        await blogService.DeleteBlogAsync(blogId);
        return NoContent();
    }

    private async Task<List<BlogImageCreateDto>> UploadImagesAsync(IFormFile? thumbnail, List<IFormFile>? images)
    {
        var uploadedImages = new List<BlogImageCreateDto>();

        if (thumbnail is not null)
        {
            var uploaded = await mediaService.UploadImageAsync(thumbnail);
            uploadedImages.Add(new BlogImageCreateDto(uploaded.Url, true));
        }

        if (images is not null && images.Count > 0)
        {
            foreach (var file in images)
            {
                if (file is null || file.Length == 0)
                {
                    continue;
                }

                var uploaded = await mediaService.UploadImageAsync(file);
                uploadedImages.Add(new BlogImageCreateDto(uploaded.Url, false));
            }
        }

        return uploadedImages;
    }
}
