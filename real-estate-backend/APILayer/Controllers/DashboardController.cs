
namespace APILayer.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/dashboard")]
public class DashboardController(IDashboardService dashboardService) : ApiController
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
