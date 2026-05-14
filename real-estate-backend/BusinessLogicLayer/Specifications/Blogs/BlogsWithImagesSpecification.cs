using BusinessLogicLayer.Specifications;
using DataAccessLayer.Entities.BlogModule;

namespace BusinessLogicLayer.Specifications.Blogs;

public sealed class BlogsWithImagesSpecification : BaseSpecifications<BlogPost>
{
    public BlogsWithImagesSpecification(BlogSpecParams @params, bool isCount = false)
        : base(b =>
            string.IsNullOrWhiteSpace(@params.Q)
                ? true
                : b.Title.Contains(@params.Q) || (b.Description != null && b.Description.Contains(@params.Q))
        )
    {
        if (!isCount)
        {
            AddInclude(b => b.Images);

            if (!string.IsNullOrWhiteSpace(@params.Sort))
            {
                switch (@params.Sort)
                {
                    case "dateAsc":
                        AddOrderBy(b => b.PublishDate);
                        break;
                    case "titleAsc":
                        AddOrderBy(b => b.Title);
                        break;
                    case "titleDesc":
                        AddOrderByDescending(b => b.Title);
                        break;
                    default:
                        AddOrderByDescending(b => b.PublishDate);
                        break;
                }
            }
            else
            {
                AddOrderByDescending(b => b.PublishDate);
            }

            ApplyPagination(@params.PageSize, @params.Page);
        }
    }

    public BlogsWithImagesSpecification(int id) : base(b => b.BlogId == id)
    {
        AddInclude(b => b.Images);
    }
}
