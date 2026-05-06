using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entities.BlogModule;

public class BlogImage : BaseEntity
{
    public int ImageId { get; set; }

    public int BlogId { get; set; }
    public BlogPost Blog { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;
    public bool IsThumbnail { get; set; }
}
