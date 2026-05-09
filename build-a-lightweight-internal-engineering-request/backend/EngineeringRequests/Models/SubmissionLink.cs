using System;

namespace CaeDashboard.EngineeringRequests.Models;

public class SubmissionLink
{
    public int Id { get; set; }
    public string OwnerUserName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? CreatedByUserName { get; set; }
}
