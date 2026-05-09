using CaeDashboard.EngineeringRequests.Dtos;
using CaeDashboard.EngineeringRequests.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaeDashboard.EngineeringRequests.Controllers;

[ApiController]
[Route("api/submission-links")]
public class SubmissionLinksController<TDbContext> : ControllerBase where TDbContext : DbContext
{
    private readonly SubmissionLinkService<TDbContext> _service;

    public SubmissionLinksController(SubmissionLinkService<TDbContext> service)
    {
        _service = service;
    }

    private string CurrentUserName => User?.Identity?.Name ?? "UnknownUser";

    [HttpGet("mine")]
    public async Task<ActionResult<SubmissionLinkDto>> GetMySubmissionLink(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetOrCreateMyLinkAsync(CurrentUserName, cancellationToken));
    }

    [HttpGet("{token}")]
    public async Task<ActionResult<SubmissionLinkPublicDto>> GetPublicSubmissionLink(string token, CancellationToken cancellationToken)
    {
        var link = await _service.GetPublicLinkAsync(token, cancellationToken);
        return link is null ? NotFound() : Ok(link);
    }
}
