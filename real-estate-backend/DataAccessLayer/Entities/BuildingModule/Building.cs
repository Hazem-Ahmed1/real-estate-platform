using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entities.BuildingModule;

public class Building : AuditableEntity
{
    public int BuildingId { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Defines whether this building holds Sale/Sold units or Rent/Rented units.
    /// Once units are added the type must not change.
    /// </summary>
    public BuildingType Type { get; set; } = BuildingType.Sale;

    /// <summary>
    /// Derived status for this building based on its units:
    ///   Sale building → Sale or Sold
    ///   Rent building → Rent or Rented
    /// Null when no units exist yet.
    /// </summary>
    public UnitStatus? Status { get; set; }
    [RangeAttribute(0,30)]
    public int? FloorCount { get; set; }

    [Required]
    [RangeAttribute(0, double.MaxValue)]
    public double MaxArea { get; set; }

    [Required]
    [RangeAttribute(0, double.MaxValue)]
    public double BuildingArea { get; set; }

    public ICollection<Unit> Units { get; set; } = new List<Unit>();
}
