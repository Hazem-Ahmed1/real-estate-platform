namespace BusinessLogicLayer.Dtos.ProjectModule;

public class ProjectUpdateDto : ProjectCreateDto
{
    public List<int> DeletedMediaIds { get; set; } = new();
    public bool IsStatusChanged { get; set; }
}
