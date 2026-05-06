using System;
using System.Collections.Generic;

namespace CaeDashboard.EngineeringRequests.Models;

public class EngineeringRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public string? RequestedBy { get; set; }
    public string? Department { get; set; }
    public RequestPriority Priority { get; set; } = RequestPriority.P3;
    public RequestStatus Status { get; set; } = RequestStatus.Incoming;
    public RequestType Type { get; set; } = RequestType.Support;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public ICollection<RequestNote> RequestNotes { get; set; } = new List<RequestNote>();
    public ICollection<RequestAttachment> Attachments { get; set; } = new List<RequestAttachment>();
    public ICollection<RequestHistory> History { get; set; } = new List<RequestHistory>();
}
