namespace APILayer.Controllers;

[ApiController]
[Route("api/buildings")]
public class BuildingsController(IBuildingService buildingService) : ApiController
{
    #region Public Endpoints

    [HttpGet]
    public async Task<ActionResult> GetAllBuildings([FromQuery] int? projectId)
    {
        IReadOnlyList<BuildingDto>? result = await buildingService.GetPublicBuildingsAsync(projectId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetBuilding(int id)
    {
        var result = await buildingService.GetPublicBuildingByIdAsync(id);
        return Ok(result);
    }

    #endregion

    #region Admin Endpoints

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<ActionResult> GetAllAdminBuildings([FromQuery] int? projectId)
    {
        var result = await buildingService.GetAllBuildingsAsync(projectId);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/{id}")]
    public async Task<ActionResult> GetAdminBuilding(int id)
    {
        var result = await buildingService.GetBuildingByIdAsync(id);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> CreateBuilding(BuildingUpsertDto dto)
    {
        var result = await buildingService.CreateBuildingAsync(dto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateBuilding(int id, BuildingUpsertDto dto)
    {
        var result = await buildingService.UpdateBuildingAsync(id, dto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBuilding(int id)
    {
        await buildingService.DeleteBuildingAsync(id);
        return NoContent();
    }

    #endregion
}
