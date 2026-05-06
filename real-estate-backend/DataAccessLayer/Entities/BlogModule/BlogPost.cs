using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entities.BlogModule;

public class BlogPost : AuditableEntity
{
    public int BlogId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime PublishDate { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }

    public ICollection<BlogImage> Images { get; set; } = new List<BlogImage>();
}
