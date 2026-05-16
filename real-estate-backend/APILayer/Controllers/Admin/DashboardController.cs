using BusinessLogicLayer.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/dashboard")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        var result = await dashboardService.GetStatsAsync();
        return Ok(result);
    }

    [HttpGet("chart")]
    public async Task<ActionResult> GetChart()
    {
        var result = await dashboardService.GetChartAsync();
        return Ok(result);
    }
}
