using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.BuildingModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers.Admin;

[ApiController]
[Route("api/admin/buildings")]
[Authorize(Roles = "Admin")]
public class BuildingsController(IBuildingService buildingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAllAdminBuildings([FromQuery] int? projectId)
    {
        var result = await buildingService.GetAllBuildingsAsync(projectId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetAdminBuilding(int id)
    {
        var result = await buildingService.GetBuildingByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> CreateBuilding(BuildingUpsertDto dto)
    {
        var result = await buildingService.CreateBuildingAsync(dto);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateBuilding(int id, BuildingUpsertDto dto)
    {
        var result = await buildingService.UpdateBuildingAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBuilding(int id)
    {
        await buildingService.DeleteBuildingAsync(id);
        return NoContent();
    }
}
