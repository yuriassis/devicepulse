using DevicePulse.Api.Contracts;
using DevicePulse.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevicePulse.Api.Controllers;

[ApiController]
[Route("api/alerts")]
public sealed class AlertsController(IAlertService alertService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<AlertResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AlertResponse>> Create(CreateAlertRequest request, CancellationToken cancellationToken)
    {
        var alert = await alertService.CreateAsync(request, cancellationToken);
        return Created($"/api/alerts/{alert.Id}", alert);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AlertResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AlertResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await alertService.GetAllAsync(cancellationToken));

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken) =>
        await alertService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
