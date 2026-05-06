using System;

namespace CaeDashboard.EngineeringRequests.Models;

public class RequestAttachment
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public EngineeringRequest? Request { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
}
