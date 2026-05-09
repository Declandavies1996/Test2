using CaeDashboard.EngineeringRequests.Dtos;
using CaeDashboard.EngineeringRequests.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaeDashboard.EngineeringRequests.Controllers;

[ApiController]
[Route("api/recurring-issues")]
public class RecurringIssuesController<TDbContext> : ControllerBase where TDbContext : DbContext
{
    private readonly RecurringIssueService<TDbContext> _service;

    public RecurringIssuesController(RecurringIssueService<TDbContext> service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecurringIssueDto>>> GetIssues(
        [FromQuery] string? search,
        [FromQuery] string? system,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetIssuesAsync(search, system, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult> CreateIssue([FromBody] UpsertRecurringIssueDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.SystemName) || string.IsNullOrWhiteSpace(dto.IssueSummary))
        {
            return BadRequest("SystemName and IssueSummary are required.");
        }

        var id = await _service.CreateIssueAsync(dto, cancellationToken);
        return Ok(new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateIssue(int id, [FromBody] UpsertRecurringIssueDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.SystemName) || string.IsNullOrWhiteSpace(dto.IssueSummary))
        {
            return BadRequest("SystemName and IssueSummary are required.");
        }

        var updated = await _service.UpdateIssueAsync(id, dto, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteIssue(int id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteIssueAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
