using Microsoft.AspNetCore.Http;

namespace APILayer.Dtos.Blogs;

public sealed class BlogUpdateFormDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? PublishDate { get; set; }

    public IFormFile? Thumbnail { get; set; }
    public List<IFormFile>? Images { get; set; }
}
