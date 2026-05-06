using System;

namespace CaeDashboard.EngineeringRequests.Models;

public class RequestHistory
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public EngineeringRequest? Request { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
}
