using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.BlogModule;
using BusinessLogicLayer.Exceptions;
using BusinessLogicLayer.Specifications.Blogs;
using DataAccessLayer.Common;
using DataAccessLayer.Contracts;
using DataAccessLayer.Entities.BlogModule;

namespace BusinessLogicLayer.Implementation;

public class BlogService(IUnitOfWork unitOfWork) : IBlogService
{
    public async Task<PaginatedResult<BlogListDto>> GetBlogsAsync(BlogSpecParams @params)
    {
        var spec = new BlogsWithImagesSpecification(@params);
        var countSpec = new BlogsWithImagesSpecification(@params, isCount: true);

        var totalItems = await unitOfWork.Repository<BlogPost>().CountAsync(countSpec);
        var blogs = await unitOfWork.Repository<BlogPost>().GetAllAsync(spec);

        var data = blogs
            .Select(b => new BlogListDto(
                b.BlogId,
                b.Title,
                b.PublishDate,
                b.Description,
                b.Images
                    .Where(i => i.IsThumbnail)
                    .Select(i => new BlogImageItemDto(i.ImageId, i.ImageUrl))
                    .FirstOrDefault(),
                b.Images
                    .Where(i => !i.IsThumbnail)
                    .Select(i => new BlogImageItemDto(i.ImageId, i.ImageUrl))
                    .ToList()
            ))
            .ToList();

        return new PaginatedResult<BlogListDto>(@params.Page, @params.PageSize, totalItems, data);
    }

    public async Task<BlogDetailsDto> GetBlogByIdAsync(int id)
    {
        var spec = new BlogsWithImagesSpecification(id);
        var blog = await unitOfWork.Repository<BlogPost>().GetByIdAsync(spec);

        if (blog is null)
        {
            throw new NotFoundExpection(nameof(BlogPost), id);
        }

        return new BlogDetailsDto(
            blog.BlogId,
            blog.Title,
            blog.PublishDate,
            blog.Description,
            blog.Images
                .Where(i => i.IsThumbnail)
                .Select(i => new BlogImageItemDto(i.ImageId, i.ImageUrl))
                .FirstOrDefault(),
            blog.Images
                .Where(i => !i.IsThumbnail)
                .Select(i => new BlogImageItemDto(i.ImageId, i.ImageUrl))
                .ToList()
        );
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
                .Where(i => i.IsThumbnail)
                .Select(i => new BlogImageItemDto(i.ImageId, i.ImageUrl))
                .FirstOrDefault(),
            blog.Images
                .Where(i => !i.IsThumbnail)
                .Select(i => new BlogImageItemDto(i.ImageId, i.ImageUrl))
                .ToList()
        );
    }

    public async Task<BlogDetailsDto> UpdateBlogAsync(int id, BlogUpdateDto dto)
    {
        var spec = new BlogsWithImagesSpecification(id);
        var blog = await unitOfWork.Repository<BlogPost>().GetByIdAsync(spec);

        if (blog is null)
        {
            throw new NotFoundExpection(nameof(BlogPost), id);
        }

        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            blog.Title = dto.Title.Trim();
        }

        if (dto.Description is not null)
        {
            blog.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        }

        if (dto.PublishDate.HasValue)
        {
            blog.PublishDate = dto.PublishDate.Value;
        }

        if (dto.Images is not null)
        {
            var thumbCount = dto.Images.Count(i => i.IsThumbnail);
            if (thumbCount > 1)
            {
                throw new BadRequestException("Only one thumbnail image is allowed.");
            }

            blog.Images.Clear();

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

        unitOfWork.Repository<BlogPost>().Update(blog);
        await unitOfWork.CompleteAsync();

        return new BlogDetailsDto(
            blog.BlogId,
            blog.Title,
            blog.PublishDate,
            blog.Description,
            blog.Images
                .Where(i => i.IsThumbnail)
                .Select(i => new BlogImageItemDto(i.ImageId, i.ImageUrl))
                .FirstOrDefault(),
            blog.Images
                .Where(i => !i.IsThumbnail)
                .Select(i => new BlogImageItemDto(i.ImageId, i.ImageUrl))
                .ToList()
        );
    }

    public async Task DeleteBlogAsync(int id)
    {
        var spec = new BlogsWithImagesSpecification(id);
        var blog = await unitOfWork.Repository<BlogPost>().GetByIdAsync(spec);

        if (blog is null)
        {
            throw new NotFoundExpection(nameof(BlogPost), id);
        }

        if (blog.Images.Count > 0)
        {
            var imageRepo = unitOfWork.Repository<BlogImage>();
            foreach (var image in blog.Images.ToList())
            {
                imageRepo.Remove(image);
            }
        }

        unitOfWork.Repository<BlogPost>().Remove(blog);
        await unitOfWork.CompleteAsync();
    }
}
