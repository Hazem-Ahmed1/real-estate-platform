namespace APILayer.Controllers;

[ApiController]
[Route("api/units")]
public class UnitsController(IUnitService unitService) : ApiController
{
    #region Public Endpoints

    [HttpGet]
    public async Task<ActionResult> GetUnits([FromQuery] UnitSpecParams @params)
    {
        @params.PublicOnly = true;
        PaginatedResult<UnitListDto>? result = await unitService.GetUnitsAsync(@params);
        foreach (var item in result.Data) item.IsStatusChanged = null;
        return Ok(result);

    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetUnitDto>> GetUnit(int id)
    {
        var unit = await unitService.GetUnitByIdAsync(id);
        
        if (unit == null)
            throw new NotFoundExpection("Unit", id);

        // Public can only see Sale and Rent units
        if (!User.IsInRole("Admin") && unit.Status != UnitStatus.Sale && unit.Status != UnitStatus.Rent)
            throw new NotFoundExpection("Unit", id);


        unit.IsStatusChanged = null; // Hide for public
        return Ok(unit);

    }

    #endregion

    #region Admin Endpoints

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<ActionResult> GetAdminUnits([FromQuery] UnitSpecParams @params)
    {
        var result = await unitService.GetUnitsAsync(@params);
        return Ok(result);


    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> CreateUnit([FromForm] CreateUnitDto unitDto)
    {
        var created = await unitService.CreateUnitAsync(unitDto);
        return CreatedAtAction(nameof(GetUnit), new { id = created.UnitId }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUnit(int id, [FromForm] UpdateUnitDto unitDto)
    {
        var result = await unitService.UpdateUnitAsync(id, unitDto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUnit(int id)
    {
        await unitService.DeleteUnitAsync(id);
        return NoContent();
    }

    #endregion
}
