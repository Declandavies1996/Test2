using CaeDashboard.EngineeringRequests.Models;

namespace CaeDashboard.EngineeringRequests.Dtos;

public record EngineeringSystemDto(
    int Id,
    string Name,
    string? Purpose,
    string? MainUsers,
    SystemCriticality Criticality,
    string? KnownRisks,
    string? Notes);

public record UpsertEngineeringSystemDto(
    string Name,
    string? Purpose,
    string? MainUsers,
    SystemCriticality Criticality,
    string? KnownRisks,
    string? Notes);
