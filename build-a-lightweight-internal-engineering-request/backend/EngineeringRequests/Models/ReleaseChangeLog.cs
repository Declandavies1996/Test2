using System;

namespace CaeDashboard.EngineeringRequests.Models;

public class ReleaseChangeLog
{
    public int Id { get; set; }
    public int? RequestId { get; set; }
    public EngineeringRequest? Request { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;
    public string Summary { get; set; } = string.Empty;
    public string? FilesChanged { get; set; }
    public string? DeploymentNotes { get; set; }
    public string? RollbackNotes { get; set; }
    public string? VerifiedBy { get; set; }
}
