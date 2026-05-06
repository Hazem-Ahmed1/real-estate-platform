using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.BlogModule;
using BusinessLogicLayer.Exceptions;
using BusinessLogicLayer.Specifications.Blogs;
using DataAccessLayer.Contracts;
using DataAccessLayer.Entities.BlogModule;

namespace BusinessLogicLayer.Implementation;

public class BlogService(IUnitOfWork unitOfWork) : IBlogService
{
    public async Task<IReadOnlyList<BlogListDto>> GetBlogsAsync()
    {
        var spec = new BlogsWithImagesSpecification();
        var blogs = await unitOfWork.Repository<BlogPost>().GetAllAsync(spec);

        return blogs
            .Select(b => new BlogListDto(
                b.BlogId,
                b.Title,
                b.PublishDate,
                b.Description,
                b.Images.FirstOrDefault(i => i.IsThumbnail)?.ImageUrl
            ))
            .ToList();
    }

    public async Task<BlogDetailsDto> CreateBlogAsync(BlogCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new BadRequestException("Title is required.");
        }

        if (dto.Images is not null)
        {
            var thumbCount = dto.Images.Count(i => i.IsThumbnail);
            if (thumbCount > 1)
            {
                throw new BadRequestException("Only one thumbnail image is allowed.");
            }
        }

        var blog = new BlogPost
        {
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            PublishDate = dto.PublishDate ?? DateTime.UtcNow,
            Images = new List<BlogImage>()
        };

        if (dto.Images is not null)
        {
            foreach (var image in dto.Images)
            {
                if (string.IsNullOrWhiteSpace(image.ImageUrl))
                {
                    continue;
                }

                blog.Images.Add(new BlogImage
                {
                    ImageUrl = image.ImageUrl.Trim(),
                    IsThumbnail = image.IsThumbnail
                });
            }
        }

        await unitOfWork.Repository<BlogPost>().AddAsync(blog);
        await unitOfWork.CompleteAsync();

        return new BlogDetailsDto(
            blog.BlogId,
            blog.Title,
            blog.PublishDate,
            blog.Description,
            blog.Images
                .Select(i => new BlogImageDto(i.ImageId, i.ImageUrl, i.IsThumbnail))
                .ToList()
        );
    }
}
