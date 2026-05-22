namespace DataAccessLayer.Entities.ProjectModule;

public class ProjectInsurance: BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int InsuranceId { get; set; }
    public Insurance Insurance { get; set; } = null!;
}




