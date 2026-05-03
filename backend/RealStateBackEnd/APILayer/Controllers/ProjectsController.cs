using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.ProjectModule;
using BusinessLogicLayer.Specifications.Projects;
using DataAccessLayer.Common;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers;

public class ProjectsController(IProjectService projectService) : ApiController
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ProjectListDto>>> GetProjects([FromQuery] ProjectSpecParams @params)
    {
        var result = await projectService.GetProjectsAsync(@params);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDetailsDto>> GetProject(int id)
    {
        var project = await projectService.GetProjectByIdAsync(id);
        return Ok(project);
    }
}
