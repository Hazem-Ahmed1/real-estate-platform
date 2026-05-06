using Microsoft.AspNetCore.Http;

namespace APILayer.Dtos.Blogs;

public sealed class BlogCreateFormDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? PublishDate { get; set; }

    // Optional: a single thumbnail image
    public IFormFile? Thumbnail { get; set; }

    // Optional: additional images (send multiple parts with the same name: Images)
    public List<IFormFile>? Images { get; set; }
}
