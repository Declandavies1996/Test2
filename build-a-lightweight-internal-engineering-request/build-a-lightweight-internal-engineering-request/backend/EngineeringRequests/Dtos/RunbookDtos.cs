using System;
using CaeDashboard.EngineeringRequests.Models;

namespace CaeDashboard.EngineeringRequests.Dtos;

public record RunbookListItemDto(
    int Id,
    string Title,
    string SystemName,
    RunbookCategory Category,
    DateTime UpdatedDate);

public record RunbookDetailDto(
    int Id,
    string Title,
    string SystemName,
    RunbookCategory Category,
    string? Symptoms,
    string? Cause,
    string? ResolutionSteps,
    string? VerificationSteps,
    string? KnownRisks,
    string? Notes,
    DateTime CreatedDate,
    DateTime UpdatedDate);

public record UpsertRunbookDto(
    string Title,
    string SystemName,
    RunbookCategory Category,
    string? Symptoms,
    string? Cause,
    string? ResolutionSteps,
    string? VerificationSteps,
    string? KnownRisks,
    string? Notes);

public record LinkedRunbookDto(
    int Id,
    int RunbookId,
    string Title,
    string SystemName,
    RunbookCategory Category,
    DateTime LinkedDate);

public record LinkRunbookDto(int RunbookId);
