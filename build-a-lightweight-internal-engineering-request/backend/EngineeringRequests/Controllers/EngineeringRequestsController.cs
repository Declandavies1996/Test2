using CaeDashboard.EngineeringRequests.Dtos;
using CaeDashboard.EngineeringRequests.Models;
using CaeDashboard.EngineeringRequests.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaeDashboard.EngineeringRequests.Controllers;

[ApiController]
[Route("api/engineering-requests")]
public class EngineeringRequestsController<TDbContext> : ControllerBase where TDbContext : DbContext
{
    private readonly EngineeringRequestService<TDbContext> _service;

    public EngineeringRequestsController(EngineeringRequestService<TDbContext> service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EngineeringRequestListItemDto>>> GetRequests(
        [FromQuery] string? search,
        [FromQuery] RequestStatus? status,
        [FromQuery] string? system,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetRequestsAsync(search, status, system, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EngineeringRequestDetailDto>> GetRequest(int id, CancellationToken cancellationToken)
    {
        var request = await _service.GetRequestAsync(id, cancellationToken);
        return request is null ? NotFound() : Ok(request);
    }

    [HttpPost]
    public async Task<ActionResult> CreateRequest(UpsertEngineeringRequestDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.SystemName))
        {
            return BadRequest("Title and SystemName are required.");
        }

        var id = await _service.CreateRequestAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetRequest), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateRequest(
        int id,
        UpsertEngineeringRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.SystemName))
        {
            return BadRequest("Title and SystemName are required.");
        }

        var updated = await _service.UpdateRequestAsync(id, dto, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteRequest(int id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteRequestAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/notes")]
    public async Task<ActionResult<RequestNoteDto>> AddNote(
        int id,
        AddRequestNoteDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.NoteText))
        {
            return BadRequest("NoteText is required.");
        }

        var note = await _service.AddNoteAsync(id, dto, cancellationToken);
        return note is null ? NotFound() : Ok(note);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<RequestDashboardSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetDashboardSummaryAsync(cancellationToken));
    }
}
