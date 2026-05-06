using Microsoft.AspNetCore.Http;

namespace APILayer.Dtos.Media;

public sealed class MediaUploadFormDto
{
    public IFormFile File { get; set; } = null!;
}
