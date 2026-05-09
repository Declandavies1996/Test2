using System;

namespace CaeDashboard.EngineeringRequests.Dtos;

public record RecurringIssueDto(
    int Id,
    string SystemName,
    string IssueSummary,
    int RecurrenceCount,
    string? TemporaryFix,
    string? SuspectedRootCause,
    bool PermanentFixNeeded,
    string? RelatedRequestIds,
    DateTime CreatedDate,
    DateTime UpdatedDate);

public record UpsertRecurringIssueDto(
    string SystemName,
    string IssueSummary,
    int RecurrenceCount,
    string? TemporaryFix,
    string? SuspectedRootCause,
    bool PermanentFixNeeded,
    string? RelatedRequestIds);
