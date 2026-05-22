using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.LookupModule;
using BusinessLogicLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers.Admin;

[ApiController]
[Route("api/admin/lookups")]
[Authorize(Roles = "Admin")]
public class LookupsController(ILookupService lookupService) : ControllerBase
{
    [HttpGet("features")]
    public async Task<ActionResult> GetFeatures([FromQuery] LookupStatus status = LookupStatus.Active)
    {
        var result = await lookupService.GetFeaturesAsync(status);
        return Ok(result);
    }

    [HttpGet("features/{id}")]
    public async Task<ActionResult> GetFeature(int id)
    {
        var result = await lookupService.GetFeatureByIdAsync(id);
        return Ok(result);
    }

    [HttpPost("features")]
    public async Task<ActionResult> CreateFeature(LookupUpsertDto dto)
    {
        var result = await lookupService.CreateFeatureAsync(dto);
        return Ok(result);
    }

    [HttpPut("features/{id}")]
    public async Task<ActionResult> UpdateFeature(int id, UpdateFeatureDto dto)
    {
        var result = await lookupService.UpdateFeatureAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("features/{id}")]
    public async Task<ActionResult> DeleteFeature(int id)
    {
        await lookupService.DeleteFeatureAsync(id);
        return NoContent();
    }

    [HttpGet("insurance")]
    public async Task<ActionResult> GetInsurances([FromQuery] LookupStatus status = LookupStatus.Active)
    {
        var result = await lookupService.GetInsurancesAsync(status);
        return Ok(result);
    }

    [HttpGet("insurance/{id}")]
    public async Task<ActionResult> GetInsurance(int id)
    {
        var result = await lookupService.GetInsuranceByIdAsync(id);
        return Ok(result);
    }

    [HttpPost("insurance")]
    public async Task<ActionResult> CreateInsurance(LookupUpsertDto dto)
    {
        var result = await lookupService.CreateInsuranceAsync(dto);
        return Ok(result);
    }

    [HttpPut("insurance/{id}")]
    public async Task<ActionResult> UpdateInsurance(int id, UpdateInsuranceDto dto)
    {
        var result = await lookupService.UpdateInsuranceAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("insurance/{id}")]
    public async Task<ActionResult> DeleteInsurance(int id)
    {
        await lookupService.DeleteInsuranceAsync(id);
        return NoContent();
    }
}
