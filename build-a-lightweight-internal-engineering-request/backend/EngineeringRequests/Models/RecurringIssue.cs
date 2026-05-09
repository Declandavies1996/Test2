using System;

namespace CaeDashboard.EngineeringRequests.Models;

public class RecurringIssue
{
    public int Id { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public string IssueSummary { get; set; } = string.Empty;
    public int RecurrenceCount { get; set; } = 1;
    public string? TemporaryFix { get; set; }
    public string? SuspectedRootCause { get; set; }
    public bool PermanentFixNeeded { get; set; }
    public string? RelatedRequestIds { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
}
