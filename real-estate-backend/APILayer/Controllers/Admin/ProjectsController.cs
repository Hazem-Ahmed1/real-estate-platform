using APILayer.Dtos.Projects;
using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.ProjectModule;
using BusinessLogicLayer.Specifications.Projects;
using DataAccessLayer.Common;
using DataAccessLayer.Entities.ProjectModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers.Admin;

[ApiController]
[Route("api/admin/projects")]
[Authorize(Roles = "Admin")]
public class ProjectsController(IProjectService projectService, IMediaService mediaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetProjects([FromQuery] ProjectSpecParams @params)
    {
        var result = await projectService.GetProjectsAsync(@params);
        return Ok(result);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> CreateProject([FromForm] ProjectCreateFormDto form)
    {
        var dto = new ProjectCreateDto
        {
            Name = form.Name,
            City = form.City,
            Area = form.Area,
            Address = form.Address,
            Latitude = form.Latitude,
            Longitude = form.Longitude,
            LandArea = form.LandArea,
            BuildUpArea = form.BuildUpArea,
            TotalBuildingArea = form.TotalBuildingArea,
            Status = form.Status,
            FeatureIds = form.FeatureIds,
            InsuranceIds = form.InsuranceIds,
            ThumbnailImage = form.ThumbnailImage,
            Images = form.Images,
            Panorama360 = form.Panorama360,
            VideoFile = form.VideoFile
        };

        var created = await projectService.CreateProjectAsync(dto);
        return CreatedAtAction("GetProject", "Projects", new { id = created.ProjectId }, created);
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> UpdateProject(int id, [FromForm] ProjectUpdateFormDto form)
    {
        var dto = new ProjectUpdateDto
        {
            Name = form.Name,
            City = form.City,
            Area = form.Area,
            Address = form.Address,
            Latitude = form.Latitude,
            Longitude = form.Longitude,
            LandArea = form.LandArea,
            BuildUpArea = form.BuildUpArea,
            TotalBuildingArea = form.TotalBuildingArea,
            Status = form.Status,
            FeatureIds = form.FeatureIds,
            InsuranceIds = form.InsuranceIds,
            DeletedMediaIds = form.DeletedMediaIds,
            IsStatusChanged = form.IsStatusChanged,
            ThumbnailImage = form.ThumbnailImage,
            Images = form.Images,
            Panorama360 = form.Panorama360,
            VideoFile = form.VideoFile
        };

        var result = await projectService.UpdateProjectAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProject(int id)
    {
        await projectService.DeleteProjectAsync(id);
        return NoContent();
    }
}
