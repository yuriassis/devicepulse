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
}
