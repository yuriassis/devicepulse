using DevicePulse.Api.Contracts;
using DevicePulse.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevicePulse.Api.Controllers;

[ApiController]
[Route("api/equipments")]
public sealed class EquipmentsController(IEquipmentService equipmentService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<EquipmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EquipmentResponse>> Create(
        CreateEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var equipment = await equipmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = equipment.Id }, equipment);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EquipmentResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EquipmentResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var equipments = await equipmentService.GetAllAsync(cancellationToken);
        return Ok(equipments);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType<EquipmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipmentResponse>> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var equipment = await equipmentService.GetByIdAsync(id, cancellationToken);
        return equipment is null ? NotFound() : Ok(equipment);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType<EquipmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EquipmentResponse>> Update(
        long id,
        UpdateEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var equipment = await equipmentService.UpdateAsync(id, request, cancellationToken);
        return equipment is null ? NotFound() : Ok(equipment);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await equipmentService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:long}/readings")]
    [ProducesResponseType<EquipmentReadingResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipmentReadingResponse>> CreateReading(
        long id,
        CreateReadingRequest request,
        CancellationToken cancellationToken)
    {
        var reading = await equipmentService.CreateReadingAsync(id, request, cancellationToken);
        return reading is null
            ? NotFound()
            : CreatedAtAction(nameof(GetReadings), new { id, limit = 50 }, reading);
    }

    [HttpGet("{id:long}/readings")]
    [ProducesResponseType<IReadOnlyList<EquipmentReadingResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<EquipmentReadingResponse>>> GetReadings(
        long id,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var readings = await equipmentService.GetReadingsAsync(id, limit, cancellationToken);
        return readings is null ? NotFound() : Ok(readings);
    }
}
