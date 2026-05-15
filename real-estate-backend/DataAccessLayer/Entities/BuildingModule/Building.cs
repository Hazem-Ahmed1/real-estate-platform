
namespace DataAccessLayer.Entities.BuildingModule;

public class Building : AuditableEntity
{
    public int BuildingId { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string? Name { get; set; }

    public int? FloorCount { get; set; }
    public double? MaxArea { get; set; }
    public double? BuildingArea { get; set; }




    public ICollection<Unit> Units { get; set; } = new List<Unit>();
}
