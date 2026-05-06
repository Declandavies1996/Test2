namespace CaeDashboard.EngineeringRequests.Models;

public class EngineeringSystem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public string? MainUsers { get; set; }
    public SystemCriticality Criticality { get; set; } = SystemCriticality.Medium;
    public string? KnownRisks { get; set; }
    public string? Notes { get; set; }
}
