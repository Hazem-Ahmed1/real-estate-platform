
namespace DataAccessLayer.Entities.ProjectModule;

public class Project : AuditableEntity
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = null!;
    public ProjectStatus Status { get; set; } = ProjectStatus.Sale;
    public double? TotalArea { get; set; }
    public int AvailableUnitsCount { get; set; }
    public int TransactedUnitsCount { get; set; }
    public string? City { get; set; }
    public string? Area { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public bool IsStatusChanged { get; set; }
    public double? LandArea { get; set; }
    public double? BuildUpArea { get; set; }
    public double? TotalBuildingArea { get; set; }


    // Navigation Properties
    public ICollection<Building> Buildings { get; set; } = new List<Building>();
    public ICollection<ProjectMedia> Media { get; set; } = new List<ProjectMedia>();
    public ICollection<ProjectFeature> ProjectFeatures { get; set; } = new List<ProjectFeature>();
    public ICollection<ProjectInsurance> ProjectInsurance { get; set; } = new List<ProjectInsurance>();
}
