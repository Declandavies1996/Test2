using System;
using System.Collections.Generic;
using CaeDashboard.EngineeringRequests.Models;

namespace CaeDashboard.EngineeringRequests.Dtos;

public record WeeklyManagementSummaryDto(
    DateTime WeekStart,
    DateTime WeekEnd,
    IReadOnlyList<WeeklyRequestDto> CompletedRequestsThisWeek,
    IReadOnlyList<WeeklyRequestDto> NewRequestsThisWeek,
    IReadOnlyList<WeeklyRequestDto> OpenP1P2Requests,
    IReadOnlyList<WaitingRequestDto> BlockedWaitingRequests,
    IReadOnlyList<OpenRequestsBySystemDto> SystemsCausingMostWork,
    IReadOnlyList<SystemRiskSummaryDto> KeySystemRisks,
    IReadOnlyList<string> TalkingPoints);

public record WeeklyRequestDto(
    int Id,
    string Title,
    string SystemName,
    RequestPriority Priority,
    RequestStatus Status,
    RequestType Type,
    DateTime CreatedDate,
    DateTime UpdatedDate);

public record SystemRiskDashboardDto(
    IReadOnlyList<SystemRiskSummaryDto> HighRiskSystems,
    IReadOnlyList<SystemWorkloadRiskDto> SystemsWithMostOpenRequests,
    IReadOnlyList<SystemWorkloadRiskDto> SystemsWithMostP1P2Requests,
    IReadOnlyList<SystemRecurringIssueRiskDto> SystemsWithRepeatedIssues,
    IReadOnlyList<SystemRiskSummaryDto> SystemsWithKnownRisks);

public record SystemRiskSummaryDto(
    int Id,
    string Name,
    string? Purpose,
    string? MainUsers,
    SystemCriticality Criticality,
    string? KnownRisks,
    string? Notes);

public record SystemWorkloadRiskDto(
    string SystemName,
    int OpenCount,
    int P1Count,
    int P2Count);

public record SystemRecurringIssueRiskDto(
    string SystemName,
    int IssueCount,
    int TotalRecurrences,
    bool PermanentFixNeeded);
