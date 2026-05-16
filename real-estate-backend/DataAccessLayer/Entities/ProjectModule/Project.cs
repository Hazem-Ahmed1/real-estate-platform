using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.ProjectModule;

public class Project : AuditableEntity
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = null!;
    public ProjectStatus Status { get; set; } = ProjectStatus.Sale;
    public int AvailableUnitsCount { get; set; }
    public int TransactedUnitsCount { get; set; }
    [Required]
    [StringLength(100)]
    public string City { get; set; } = null!;

    [StringLength(200)]
    public string? Area { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [Required]
    [RangeAttribute(-90, 90)]
    public double Latitude { get; set; }

    [Required]
    [RangeAttribute(-180, 180)]
    public double Longitude { get; set; }

    public bool IsStatusChanged { get; set; }
    [Required]
    [RangeAttribute(0, double.MaxValue)]
    public double LandArea { get; set; }

    [Required]
    [RangeAttribute(0, double.MaxValue)]
    public double BuildUpArea { get; set; }

    public double? TotalBuildingArea { get; set; }

    public ICollection<Building> Buildings { get; set; } = new List<Building>();
    public ICollection<ProjectMedia> Media { get; set; } = new List<ProjectMedia>();
    public ICollection<ProjectFeature> ProjectFeatures { get; set; } = new List<ProjectFeature>();
    public ICollection<ProjectInsurance> ProjectInsurance { get; set; } = new List<ProjectInsurance>();
}
