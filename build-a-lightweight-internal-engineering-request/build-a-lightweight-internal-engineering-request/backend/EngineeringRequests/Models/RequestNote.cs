using System;

namespace CaeDashboard.EngineeringRequests.Models;

public class RequestNote
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public EngineeringRequest? Request { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
