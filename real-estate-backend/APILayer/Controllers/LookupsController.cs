namespace APILayer.Controllers;

[ApiController]
[Route("api/lookups")]
public class LookupsController(ILookupService lookupService) : ApiController
{
    #region Public Lookups

    [HttpGet("features/public")]
    public async Task<ActionResult> GetPublicFeatures()
    {
        var result = await lookupService.GetFeaturesAsync(LookupStatus.Active);
        return Ok(result);
    }

    [HttpGet("insurance/public")]
    public async Task<ActionResult> GetPublicInsurances()
    {
        var result = await lookupService.GetInsurancesAsync(LookupStatus.Active);
        return Ok(result);
    }

    [HttpGet("sort-options")]
    public async Task<ActionResult> GetSortOptions()
    {
        // This could be made more dynamic by passing an entity name, 
        // but for now we provide the project-level sort options globally.
        var result = new List<string> { "priceAsc", "priceDesc", "newest", "areaAsc", "areaDesc" };
        return Ok(result);
    }

    [HttpGet("cities")]
    public async Task<ActionResult> GetCities([FromServices] IProjectService projectService)
    {
        var result = await projectService.GetAvailableCitiesAsync();
        return Ok(result);
    }

    #endregion

    #region Admin Endpoints

    [Authorize(Roles = "Admin")]
    [HttpGet("features")]
    public async Task<ActionResult> GetFeatures([FromQuery] LookupStatus status = LookupStatus.Active)
    {
        var result = await lookupService.GetFeaturesAsync(status);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("features/{id}")]
    public async Task<ActionResult> GetFeature(int id)
    {
        var result = await lookupService.GetFeatureByIdAsync(id);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("features")]
    public async Task<ActionResult> CreateFeature(LookupUpsertDto dto)
    {
        var result = await lookupService.CreateFeatureAsync(dto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("features/{id}")]
    public async Task<ActionResult> UpdateFeature(int id, UpdateFeatureDto dto)
    {
        var result = await lookupService.UpdateFeatureAsync(id, dto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("features/{id}")]
    public async Task<ActionResult> DeleteFeature(int id)
    {
        await lookupService.DeleteFeatureAsync(id);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("insurance")]
    public async Task<ActionResult> GetInsurances([FromQuery] LookupStatus status = LookupStatus.Active)
    {
        var result = await lookupService.GetInsurancesAsync(status);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("insurance/{id}")]
    public async Task<ActionResult> GetInsurance(int id)
    {
        var result = await lookupService.GetInsuranceByIdAsync(id);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("insurance")]
    public async Task<ActionResult> CreateInsurance(LookupUpsertDto dto)
    {
        var result = await lookupService.CreateInsuranceAsync(dto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("insurance/{id}")]
    public async Task<ActionResult> UpdateInsurance(int id, UpdateInsuranceDto dto)
    {
        var result = await lookupService.UpdateInsuranceAsync(id, dto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("insurance/{id}")]
    public async Task<ActionResult> DeleteInsurance(int id)
    {
        await lookupService.DeleteInsuranceAsync(id);
        return NoContent();
    }

    #endregion
}
