using System;

namespace CaeDashboard.EngineeringRequests.Models;

public class RequestRunbook
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public EngineeringRequest? Request { get; set; }
    public int RunbookId { get; set; }
    public Runbook? Runbook { get; set; }
    public DateTime LinkedDate { get; set; } = DateTime.UtcNow;
}
