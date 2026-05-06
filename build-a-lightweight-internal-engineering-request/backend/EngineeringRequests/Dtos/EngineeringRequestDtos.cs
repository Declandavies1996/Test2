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
    IReadOnlyList<RequestNoteDto> RequestNotes);

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

public record RequestDashboardSummaryDto(
    int OpenP1Count,
    int RequestsThisWeek,
    IReadOnlyList<GroupCountDto> RequestsBySystem,
    IReadOnlyList<GroupCountDto> RequestsByType);

public record GroupCountDto(string Name, int Count);
