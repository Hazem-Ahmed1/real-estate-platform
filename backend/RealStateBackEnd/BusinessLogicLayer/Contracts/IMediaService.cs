using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Contracts;

public interface IMediaService
{
    Task<string> UploadImageAsync(IFormFile file);
    Task<bool> DeleteImageAsync(string publicId);
}
