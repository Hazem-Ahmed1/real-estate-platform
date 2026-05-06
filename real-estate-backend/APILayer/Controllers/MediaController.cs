using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.MediaModule;
using Microsoft.AspNetCore.Mvc;
using APILayer.Dtos.Media;

namespace APILayer.Controllers;

public class MediaController(IMediaService mediaService) : ApiController
{
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<MediaUploadResultDto>> Upload([FromForm] MediaUploadFormDto form)
    {
        var file = form.File;
        if (file is null || file.Length == 0)
        {
            return BadRequest("File is required.");
        }

        if (string.IsNullOrWhiteSpace(file.ContentType) || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only image files are allowed.");
        }

        var result = await mediaService.UploadImageAsync(file);
        return Ok(result);
    }
}
