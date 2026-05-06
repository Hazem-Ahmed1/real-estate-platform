using BusinessLogicLayer.Specifications;
using DataAccessLayer.Entities.BlogModule;

namespace BusinessLogicLayer.Specifications.Blogs;

public sealed class BlogsWithImagesSpecification : BaseSpecifications<BlogPost>
{
    public BlogsWithImagesSpecification()
    {
        AddInclude(b => b.Images);
        AddOrderByDescending(b => b.PublishDate);
    }
}
