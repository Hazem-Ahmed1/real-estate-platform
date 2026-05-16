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
        foreach (var item in result.Items) item.IsStatusChanged = null;
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
}

