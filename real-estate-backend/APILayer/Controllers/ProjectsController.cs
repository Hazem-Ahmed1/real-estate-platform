using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.ProjectModule;
using BusinessLogicLayer.Dtos.UnitModule;
using BusinessLogicLayer.Exceptions;
using BusinessLogicLayer.Specifications.Projects;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController(IProjectService projectService, IUnitService unitService) : ApiController
{
    #region Public Endpoints

    [HttpGet]
    public async Task<ActionResult> GetProjects([FromQuery] ProjectSpecParams @params)
    {
        var result = await projectService.GetProjectsAsync(@params);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDetailsDto>> GetProject(int id)
    {
        var project = await projectService.GetProjectByIdAsync(id);
        if (project == null) throw new NotFoundExpection("Project", id);
        return Ok(project);
    }

    [HttpGet("{id}/units")]
    public async Task<ActionResult<IReadOnlyList<UnitListDto>>> GetProjectUnits(int id)
    {
        var units = await unitService.GetProjectUnitsAsync(id);
        return Ok(units);
    }

    #endregion
}
