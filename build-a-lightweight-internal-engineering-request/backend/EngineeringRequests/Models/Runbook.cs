using System;
using System.Collections.Generic;

namespace CaeDashboard.EngineeringRequests.Models;

public class Runbook
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public RunbookCategory Category { get; set; } = RunbookCategory.Other;
    public string? Symptoms { get; set; }
    public string? Cause { get; set; }
    public string? ResolutionSteps { get; set; }
    public string? VerificationSteps { get; set; }
    public string? KnownRisks { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<RequestRunbook> RequestRunbooks { get; set; } = new List<RequestRunbook>();
}
