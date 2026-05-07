using CaeDashboard.EngineeringRequests.Dtos;
using CaeDashboard.EngineeringRequests.Models;
using CaeDashboard.EngineeringRequests.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaeDashboard.EngineeringRequests.Controllers;

[ApiController]
[Route("api/runbooks")]
public class RunbooksController<TDbContext> : ControllerBase where TDbContext : DbContext
{
    private readonly RunbookService<TDbContext> _service;

    public RunbooksController(RunbookService<TDbContext> service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RunbookListItemDto>>> GetRunbooks(
        [FromQuery] string? search,
        [FromQuery] string? system,
        [FromQuery] RunbookCategory? category,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetRunbooksAsync(search, system, category, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RunbookDetailDto>> GetRunbook(int id, CancellationToken cancellationToken)
    {
        var runbook = await _service.GetRunbookAsync(id, cancellationToken);
        return runbook is null ? NotFound() : Ok(runbook);
    }

    [HttpPost]
    public async Task<ActionResult> CreateRunbook([FromBody] UpsertRunbookDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.SystemName))
        {
            return BadRequest("Title and SystemName are required.");
        }

        var id = await _service.CreateRunbookAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetRunbook), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateRunbook(
        int id,
        [FromBody] UpsertRunbookDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.SystemName))
        {
            return BadRequest("Title and SystemName are required.");
        }

        var updated = await _service.UpdateRunbookAsync(id, dto, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteRunbook(int id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteRunbookAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
