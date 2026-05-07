using System;
using System.Collections.Generic;
using CaeDashboard.EngineeringRequests.Models;

namespace CaeDashboard.EngineeringRequests.Dtos;

public record EngineeringRequestListItemDto(
    int Id,
    string Title,
    string SystemName,
    string? RequestedBy,
    string? Department,
    RequestPriority Priority,
    RequestStatus Status,
    RequestType Type,
    DateTime CreatedDate,
    DateTime UpdatedDate);

public record EngineeringRequestDetailDto(
    int Id,
    string Title,
    string? Description,
    string SystemName,
    string? RequestedBy,
    string? Department,
    RequestPriority Priority,
    RequestStatus Status,
    RequestType Type,
    DateTime CreatedDate,
    DateTime UpdatedDate,
    string? Notes,
    IReadOnlyList<RequestNoteDto> RequestNotes,
    IReadOnlyList<RequestAttachmentDto> Attachments,
    IReadOnlyList<RequestHistoryDto> History,
    IReadOnlyList<LinkedRunbookDto> LinkedRunbooks);

public record UpsertEngineeringRequestDto(
    string Title,
    string? Description,
    string SystemName,
    string? RequestedBy,
    string? Department,
    RequestPriority Priority,
    RequestStatus Status,
    RequestType Type,
    string? Notes);

public record RequestNoteDto(
    int Id,
    int RequestId,
    string NoteText,
    string? CreatedBy,
    DateTime CreatedDate);

public record AddRequestNoteDto(string NoteText, string? CreatedBy);

public record RequestAttachmentDto(
    int Id,
    int RequestId,
    string FileName,
    string StoredFileName,
    string FilePath,
    string? ContentType,
    string? UploadedBy,
    DateTime UploadedDate);

public record RequestHistoryDto(
    int Id,
    int RequestId,
    string ActionType,
    string? OldValue,
    string? NewValue,
    string? ChangedBy,
    DateTime ChangedDate);

public record RequestDashboardSummaryDto(
    int OpenP1Count,
    int RequestsThisWeek,
    IReadOnlyList<GroupCountDto> RequestsBySystem,
    IReadOnlyList<GroupCountDto> RequestsByType);

public record GroupCountDto(string Name, int Count);

public record RequestReportingFilterDto(
    DateTime? FromDate,
    DateTime? ToDate,
    string? System,
    RequestPriority? Priority,
    RequestStatus? Status,
    RequestType? Type);

public record RequestReportingDashboardDto(
    RequestReportingCardsDto Cards,
    IReadOnlyList<OpenRequestsBySystemDto> OpenRequestsBySystem,
    IReadOnlyList<RequestsByTypeDto> RequestsByType,
    IReadOnlyList<OldestOpenRequestDto> OldestOpenRequests,
    IReadOnlyList<WaitingRequestDto> WaitingRequests);

public record RequestReportingCardsDto(
    int OpenP1Requests,
    int OpenP2Requests,
    int TotalOpenRequests,
    int RequestsCreatedThisWeek,
    int RequestsCompletedThisWeek,
    int RequestsWaitingBlocked);

public record OpenRequestsBySystemDto(
    string SystemName,
    int OpenCount,
    int P1Count,
    int P2Count);

public record RequestsByTypeDto(
    RequestType Type,
    int Count);

public record OldestOpenRequestDto(
    int Id,
    string Title,
    string SystemName,
    RequestPriority Priority,
    RequestStatus Status,
    DateTime CreatedDate,
    int AgeInDays);

public record WaitingRequestDto(
    int Id,
    string Title,
    string SystemName,
    string? Reason,
    DateTime UpdatedDate);
