using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.LookupModule;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers;

[ApiController]
[Route("api/filters")]
public class FiltersController(IFilterOptionsService filterOptionsService) : ControllerBase
{
    [HttpGet("search-options")]
    public async Task<ActionResult<SearchFilterOptionsDto>> GetSearchOptions()
    {
        var options = await filterOptionsService.GetSearchFilterOptionsAsync();
        return Ok(options);
    }
}
