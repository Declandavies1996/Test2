using CaeDashboard.EngineeringRequests.Dtos;
using CaeDashboard.EngineeringRequests.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaeDashboard.EngineeringRequests.Controllers;

[ApiController]
[Route("api/release-change-logs")]
public class ReleaseChangeLogsController<TDbContext> : ControllerBase where TDbContext : DbContext
{
    private readonly ReleaseChangeLogService<TDbContext> _service;

    public ReleaseChangeLogsController(ReleaseChangeLogService<TDbContext> service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReleaseChangeLogDto>>> GetLogs(
        [FromQuery] string? system,
        [FromQuery] int? requestId,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetLogsAsync(system, requestId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult> CreateLog([FromBody] UpsertReleaseChangeLogDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.SystemName) || string.IsNullOrWhiteSpace(dto.Summary))
        {
            return BadRequest("SystemName and Summary are required.");
        }

        var id = await _service.CreateLogAsync(dto, cancellationToken);
        return Ok(new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateLog(int id, [FromBody] UpsertReleaseChangeLogDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.SystemName) || string.IsNullOrWhiteSpace(dto.Summary))
        {
            return BadRequest("SystemName and Summary are required.");
        }

        var updated = await _service.UpdateLogAsync(id, dto, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteLog(int id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteLogAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
