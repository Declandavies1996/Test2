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
    private readonly SubmissionLinkService<TDbContext> _submissionLinkService;

    public EngineeringRequestsController(
        EngineeringRequestService<TDbContext> service,
        SubmissionLinkService<TDbContext> submissionLinkService)
    {
        _service = service;
        _submissionLinkService = submissionLinkService;
    }

    private string CurrentUserName => User?.Identity?.Name ?? "UnknownUser";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EngineeringRequestListItemDto>>> GetRequests(
        [FromQuery] string? search,
        [FromQuery] RequestStatus? status,
        [FromQuery] string? system,
        [FromQuery] bool allRequests,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetRequestsAsync(search, status, system, CurrentUserName, !allRequests, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EngineeringRequestDetailDto>> GetRequest(int id, CancellationToken cancellationToken)
    {
        var request = await _service.GetRequestAsync(id, cancellationToken);
        return request is null ? NotFound() : Ok(request);
    }

    [HttpPost]
    public async Task<ActionResult> CreateRequest([FromBody] UpsertEngineeringRequestDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.SystemName))
        {
            return BadRequest("Title and SystemName are required.");
        }

        var id = await _service.CreateRequestAsync(dto, CurrentUserName, cancellationToken);
        return CreatedAtAction(nameof(GetRequest), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateRequest(
        int id,
        [FromBody] UpsertEngineeringRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.SystemName))
        {
            return BadRequest("Title and SystemName are required.");
        }

        var updated = await _service.UpdateRequestAsync(id, dto, CurrentUserName, cancellationToken);
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
        [FromBody] AddRequestNoteDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.NoteText))
        {
            return BadRequest("NoteText is required.");
        }

        var note = await _service.AddNoteAsync(id, dto, CurrentUserName, cancellationToken);
        return note is null ? NotFound() : Ok(note);
    }

    [HttpPost("{id:int}/attachments")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<RequestAttachmentDto>> UploadAttachment(
        int id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest("File is required.");
        }

        try
        {
            var attachment = await _service.AddAttachmentAsync(id, file, CurrentUserName, cancellationToken);
            return attachment is null ? NotFound() : Ok(attachment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("attachments/{attachmentId:int}")]
    public async Task<IActionResult> DownloadAttachment(int attachmentId, CancellationToken cancellationToken)
    {
        var attachment = await _service.GetAttachmentEntityAsync(attachmentId, cancellationToken);
        if (attachment is null || !System.IO.File.Exists(attachment.FilePath))
        {
            return NotFound();
        }

        var stream = System.IO.File.OpenRead(attachment.FilePath);
        return File(stream, attachment.ContentType ?? "application/octet-stream", attachment.FileName);
    }

    [HttpPost("submit")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<ActionResult<SubmitEngineeringRequestResultDto>> SubmitRequest(
        [FromForm] SubmitEngineeringRequestDto dto,
        [FromForm] List<IFormFile> attachments,
        [FromForm] string? ownerToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest("Title is required.");
        }

        var linkOwner = await _submissionLinkService.GetOwnerForTokenAsync(ownerToken, cancellationToken);
        if (!string.IsNullOrWhiteSpace(ownerToken) && string.IsNullOrWhiteSpace(linkOwner))
        {
            return BadRequest("The submission link is not valid or is no longer active.");
        }

        var result = await _service.SubmitRequestAsync(dto, attachments, CurrentUserName, linkOwner ?? CurrentUserName, cancellationToken);
        return Ok(result);
    }

    [HttpGet("my-submitted")]
    public async Task<ActionResult<IReadOnlyList<EngineeringRequestListItemDto>>> GetMySubmitted(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetMySubmittedRequestsAsync(CurrentUserName, cancellationToken));
    }

    [HttpGet("triage")]
    public async Task<ActionResult<IReadOnlyList<EngineeringRequestListItemDto>>> GetTriageRequests(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetTriageRequestsAsync(CurrentUserName, cancellationToken));
    }

    [HttpPost("{id:int}/triage")]
    public async Task<ActionResult> TriageRequest(
        int id,
        [FromBody] TriageEngineeringRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.SystemName))
        {
            return BadRequest("SystemName is required.");
        }

        var triaged = await _service.TriageRequestAsync(id, dto, CurrentUserName, cancellationToken);
        return triaged ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/runbooks")]
    public async Task<ActionResult<LinkedRunbookDto>> LinkRunbook(
        int id,
        [FromBody] LinkRunbookDto dto,
        [FromQuery] string? changedBy,
        CancellationToken cancellationToken)
    {
        var linked = await _service.LinkRunbookAsync(id, dto.RunbookId, changedBy, cancellationToken);
        return linked is null ? NotFound() : Ok(linked);
    }

    [HttpDelete("{id:int}/runbooks/{runbookId:int}")]
    public async Task<ActionResult> UnlinkRunbook(
        int id,
        int runbookId,
        [FromQuery] string? changedBy,
        CancellationToken cancellationToken)
    {
        var unlinked = await _service.UnlinkRunbookAsync(id, runbookId, changedBy, cancellationToken);
        return unlinked ? NoContent() : NotFound();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<RequestDashboardSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetDashboardSummaryAsync(cancellationToken));
    }

    [HttpGet("reporting")]
    public async Task<ActionResult<RequestReportingDashboardDto>> GetReportingDashboard(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? system,
        [FromQuery] RequestPriority? priority,
        [FromQuery] RequestStatus? status,
        [FromQuery] RequestType? type,
        [FromQuery] bool allRequests,
        CancellationToken cancellationToken)
    {
        var filters = new RequestReportingFilterDto(fromDate, toDate, system, priority, status, type);
        return Ok(await _service.GetReportingDashboardAsync(filters, CurrentUserName, !allRequests, cancellationToken));
    }

    [HttpGet("weekly-management-summary")]
    public async Task<ActionResult<WeeklyManagementSummaryDto>> GetWeeklyManagementSummary(
        [FromQuery] bool allRequests,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetWeeklyManagementSummaryAsync(CurrentUserName, !allRequests, cancellationToken));
    }

    [HttpGet("system-risk-dashboard")]
    public async Task<ActionResult<SystemRiskDashboardDto>> GetSystemRiskDashboard(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetSystemRiskDashboardAsync(cancellationToken));
    }
}
