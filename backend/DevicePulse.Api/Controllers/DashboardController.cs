using DevicePulse.Api.Contracts;
using DevicePulse.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevicePulse.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("summary")]
    [ProducesResponseType<DashboardSummaryResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummary(
        CancellationToken cancellationToken)
    {
        var summary = await dashboardService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }
}
