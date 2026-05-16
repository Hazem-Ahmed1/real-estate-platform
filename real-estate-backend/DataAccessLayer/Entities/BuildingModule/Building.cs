
namespace DataAccessLayer.Entities.BuildingModule;

public class Building : AuditableEntity
{
    public int BuildingId { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public int? FloorCount { get; set; }

    [Required]
    [RangeAttribute(0, double.MaxValue)]
    public double MaxArea { get; set; }

    [Required]
    [RangeAttribute(0, double.MaxValue)]
    public double BuildingArea { get; set; }




    public ICollection<Unit> Units { get; set; } = new List<Unit>();
}
