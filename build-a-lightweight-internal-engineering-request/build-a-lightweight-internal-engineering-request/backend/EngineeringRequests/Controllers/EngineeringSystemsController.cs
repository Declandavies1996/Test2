using CaeDashboard.EngineeringRequests.Dtos;
using CaeDashboard.EngineeringRequests.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaeDashboard.EngineeringRequests.Controllers;

[ApiController]
[Route("api/engineering-systems")]
public class EngineeringSystemsController<TDbContext> : ControllerBase where TDbContext : DbContext
{
    private readonly EngineeringSystemService<TDbContext> _service;

    public EngineeringSystemsController(EngineeringSystemService<TDbContext> service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EngineeringSystemDto>>> GetSystems(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetSystemsAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EngineeringSystemDto>> GetSystem(int id, CancellationToken cancellationToken)
    {
        var system = await _service.GetSystemAsync(id, cancellationToken);
        return system is null ? NotFound() : Ok(system);
    }

    [HttpPost]
    public async Task<ActionResult> CreateSystem(UpsertEngineeringSystemDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Name is required.");
        }

        var id = await _service.CreateSystemAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetSystem), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateSystem(
        int id,
        UpsertEngineeringSystemDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Name is required.");
        }

        var updated = await _service.UpdateSystemAsync(id, dto, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteSystem(int id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteSystemAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
