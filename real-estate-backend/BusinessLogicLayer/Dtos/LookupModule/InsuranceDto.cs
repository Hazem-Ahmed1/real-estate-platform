namespace BusinessLogicLayer.Dtos.LookupModule;

public class InsuranceDto
{
    public int InsuranceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Duration { get; set; }
    public bool IsActive { get; set; }
    public int UnitCount { get; set; }
    public int ProjectCount { get; set; }
}
