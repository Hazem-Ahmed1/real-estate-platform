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
        foreach (var item in result.Data) item.IsStatusChanged = null;
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

    #region Admin Endpoints

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<ActionResult> GetAdminProjects([FromQuery] ProjectSpecParams @params)
    {
        var result = await projectService.GetProjectsAsync(@params);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> CreateProject([FromForm] ProjectDto projectDto)
    {
        var created = await projectService.CreateProjectAsync(projectDto);
        return CreatedAtAction(nameof(GetProject), new { id = created.ProjectId }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProject(int id, [FromForm] ProjectDto projectDto)
    {
        var result = await projectService.UpdateProjectAsync(id, projectDto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProject(int id)
    {
        await projectService.DeleteProjectAsync(id);
        return NoContent();
    }

    #endregion
}
