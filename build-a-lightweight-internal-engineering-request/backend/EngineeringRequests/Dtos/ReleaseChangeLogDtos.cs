using System;

namespace CaeDashboard.EngineeringRequests.Dtos;

public record ReleaseChangeLogDto(
    int Id,
    int? RequestId,
    string SystemName,
    DateTime ReleaseDate,
    string Summary,
    string? FilesChanged,
    string? DeploymentNotes,
    string? RollbackNotes,
    string? VerifiedBy);

public record UpsertReleaseChangeLogDto(
    int? RequestId,
    string SystemName,
    DateTime ReleaseDate,
    string Summary,
    string? FilesChanged,
    string? DeploymentNotes,
    string? RollbackNotes,
    string? VerifiedBy);
