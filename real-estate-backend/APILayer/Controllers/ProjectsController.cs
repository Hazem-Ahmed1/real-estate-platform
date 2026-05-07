using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.ProjectModule;
using BusinessLogicLayer.Exceptions;
using BusinessLogicLayer.Specifications.Projects;
using DataAccessLayer.Enums;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers;

[ApiController] 
[Route("api/[controller]")]
public class ProjectsController(IProjectService projectService) : ApiController
{
    [HttpGet]
    public async Task<ActionResult> GetProjects([FromQuery] ProjectSpecParams @params)
    {
        if (@params.UnitStatus is UnitStatus.Sold or UnitStatus.Rented)
            throw new BadRequestException("Filtering projects by Sold/Rented units is restricted to admin endpoints.");

        var result = await projectService.GetProjectsAsync(@params);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetProjectDto>> GetProject(int id)
    {
        var project = await projectService.GetProjectByIdAsync(id);
        if (project == null) throw new NotFoundExpection("Project", id);

        return Ok(project);
    }

    [HttpGet("cities")]
    public async Task<ActionResult<List<string>>> GetCities()
    {
        return Ok(await projectService.GetAvailableCitiesAsync());
    }

    [HttpGet("sort-options")]
    public async Task<ActionResult<List<string>>> GetSortOptions()
    {
        return Ok(await projectService.GetSortOptionsAsync());
    }
}
