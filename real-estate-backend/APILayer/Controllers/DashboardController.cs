using BusinessLogicLayer.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace APILayer.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController(IDashboardService dashboardService) : ApiController
{
    [HttpGet("stats")]
    public async Task<ActionResult> GetPublicStats()
    {
        var result = await dashboardService.GetPublicStatsAsync();
        return Ok(result);
    }
}
