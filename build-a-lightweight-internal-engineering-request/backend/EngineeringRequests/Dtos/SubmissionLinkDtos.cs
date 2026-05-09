using System;

namespace CaeDashboard.EngineeringRequests.Dtos;

public record SubmissionLinkDto(
    int Id,
    string OwnerUserName,
    string Token,
    string? DisplayName,
    bool IsActive,
    DateTime CreatedDate,
    string? CreatedByUserName);

public record SubmissionLinkPublicDto(
    string Token,
    string? DisplayName);
