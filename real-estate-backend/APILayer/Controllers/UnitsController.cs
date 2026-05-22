using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.UnitModule;
using BusinessLogicLayer.Exceptions;
using BusinessLogicLayer.Specifications.Units;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers;

[ApiController]
[Route("api/units")]
public class UnitsController(IUnitService unitService) : ApiController
{
    #region Public Endpoints

    [HttpGet]
    public async Task<ActionResult> GetUnits([FromQuery] UnitSpecParams @params)
    {
        var result = await unitService.GetUnitsAsync(@params);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetUnitDto>> GetUnit(int id)
    {
        var unit = await unitService.GetUnitByIdAsync(id);
        if (unit == null) throw new NotFoundExpection("Unit", id);
        return Ok(unit);
    }

    #endregion
}
