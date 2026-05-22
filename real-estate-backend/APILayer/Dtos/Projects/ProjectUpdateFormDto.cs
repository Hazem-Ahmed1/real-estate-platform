namespace APILayer.Dtos.Projects;

public class ProjectUpdateFormDto : ProjectCreateFormDto
{
    public List<int> DeletedMediaIds { get; set; } = new();
}
