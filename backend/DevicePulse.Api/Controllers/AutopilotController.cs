using DevicePulse.Api.Contracts;
using DevicePulse.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevicePulse.Api.Controllers;

[ApiController]
[Route("api/autopilot")]
public sealed class AutopilotController(IAutopilotService autopilotService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<AutopilotStatusResponse>(StatusCodes.Status200OK)]
    public ActionResult<AutopilotStatusResponse> GetStatus() => Ok(autopilotService.GetStatus());

    [HttpPost("start")]
    [ProducesResponseType<AutopilotStatusResponse>(StatusCodes.Status200OK)]
    public ActionResult<AutopilotStatusResponse> Start() => Ok(autopilotService.Start());

    [HttpPost("stop")]
    [ProducesResponseType<AutopilotStatusResponse>(StatusCodes.Status200OK)]
    public ActionResult<AutopilotStatusResponse> Stop() => Ok(autopilotService.Stop());
}
