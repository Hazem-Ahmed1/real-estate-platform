using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entities.ProjectModule;

public class Project : AuditableEntity
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = null!;
    public ProjectStatus Status { get; set; } = ProjectStatus.AllForSoldOut;
    public double? TotalArea { get; set; }
    public string? City { get; set; }
    public string? Area { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Description { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation Properties
    public ICollection<Building> Buildings { get; set; } = new List<Building>();
    public ICollection<ProjectMedia> Media { get; set; } = new List<ProjectMedia>();
    public ICollection<ProjectFeature> ProjectFeatures { get; set; } = new List<ProjectFeature>();
    public ICollection<ProjectInsurance> ProjectInsurance { get; set; } = new List<ProjectInsurance>();
}
