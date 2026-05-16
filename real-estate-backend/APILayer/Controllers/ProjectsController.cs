namespace APILayer.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController(IProjectService projectService) : ApiController
{
    #region Public Endpoints

    [HttpGet]
    public async Task<ActionResult> GetProjects([FromQuery] ProjectSpecParams @params)
    {
        // Public restricted filtering: cannot see sold/rented units
        if (@params.UnitStatus is UnitStatus.Sold or UnitStatus.Rented)
            throw new BadRequestException("Filtering projects by Sold/Rented units is restricted to admin endpoints.");

        PaginatedResult<ProjectListDto>? result = await projectService.GetProjectsAsync(@params);
        foreach (var item in result.Items) item.IsStatusChanged = null;
        return Ok(result);

    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDetailsDto>> GetProject(int id)
    {
        var project = await projectService.GetProjectByIdAsync(id);
        if (project == null) throw new NotFoundExpection("Project", id);
        
        // Public can only see Sale and Rent projects
        if (!User.IsInRole("Admin") && project.Status != ProjectStatus.Sale && project.Status != ProjectStatus.Rent)
            throw new NotFoundExpection("Project", id);

        project.IsStatusChanged = null; // Hide for public

        return Ok(project);

    }

    #endregion
}

