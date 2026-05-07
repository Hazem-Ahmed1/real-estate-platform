using BusinessLogicLayer.Dtos.MediaModule;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Contracts;

public interface IMediaService
{
    Task<MediaUploadResultDto> UploadImageAsync(IFormFile file);
    Task<bool> DeleteImageAsync(string publicId);
}