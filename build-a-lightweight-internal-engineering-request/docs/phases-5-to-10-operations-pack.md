# Phases 5 to 10: Operations Pack

## Summary of Changes

This pack adds the remaining lightweight operational-management features:

- Phase 5: Weekly Management Summary
- Phase 6: Recurring Issues
- Phase 7: System Risk Dashboard
- Phase 8: Release / Change Log
- Phase 9: Runbook compatibility fields
- Phase 10: UI polish through clearer badges and cleaner operational pages

It keeps the request tracker small. There are no approvals, notifications, email integrations, kanban boards, or new frameworks.

## Files to Add

Backend:

- `backend/EngineeringRequests/Models/RecurringIssue.cs`
- `backend/EngineeringRequests/Models/ReleaseChangeLog.cs`
- `backend/EngineeringRequests/Dtos/RecurringIssueDtos.cs`
- `backend/EngineeringRequests/Dtos/ReleaseChangeLogDtos.cs`
- `backend/EngineeringRequests/Dtos/OperationalDashboardDtos.cs`
- `backend/EngineeringRequests/Services/RecurringIssueService.cs`
- `backend/EngineeringRequests/Services/ReleaseChangeLogService.cs`
- `backend/EngineeringRequests/Controllers/RecurringIssuesController.cs`
- `backend/EngineeringRequests/Controllers/ReleaseChangeLogsController.cs`

Frontend:

- `frontend/engineering-requests/pages/WeeklyManagementSummary.vue`
- `frontend/engineering-requests/pages/RecurringIssues.vue`
- `frontend/engineering-requests/pages/SystemRiskDashboard.vue`
- `frontend/engineering-requests/pages/ReleaseChangeLog.vue`

Docs:

- `docs/phases-5-to-10-operations-pack.md`

## Files to Modify

- `backend/EngineeringRequests/Models/EngineeringRequest.cs`
- `backend/EngineeringRequests/Models/Runbook.cs`
- `backend/EngineeringRequests/Dtos/RunbookDtos.cs`
- `backend/EngineeringRequests/EngineeringRequestsDbContextExtensions.cs`
- `backend/EngineeringRequests/Services/EngineeringRequestService.cs`
- `backend/EngineeringRequests/Services/RunbookService.cs`
- `backend/EngineeringRequests/Controllers/EngineeringRequestsController.cs`
- `backend/EngineeringRequests/EngineeringRequestsRegistration.cs`
- `frontend/engineering-requests/api.js`
- `frontend/engineering-requests/routes.js`
- `frontend/engineering-requests/pages/RunbookDetails.vue`
- `frontend/engineering-requests/components/StatusBadge.vue`
- `docs/sql-schema.sql`

## New API Endpoints

Weekly / risk:

- `GET /api/engineering-requests/weekly-management-summary`
- `GET /api/engineering-requests/system-risk-dashboard`

Recurring issues:

- `GET /api/recurring-issues?search=&system=`
- `POST /api/recurring-issues`
- `PUT /api/recurring-issues/{id}`
- `DELETE /api/recurring-issues/{id}`

Release/change logs:

- `GET /api/release-change-logs?system=&requestId=`
- `POST /api/release-change-logs`
- `PUT /api/release-change-logs/{id}`
- `DELETE /api/release-change-logs/{id}`

## Database Changes

New tables:

- `RecurringIssues`
- `ReleaseChangeLogs`

Runbook additions:

- `Problem`
- `FixSteps`
- `LastUpdated`

Migration SQL is appended to `docs/sql-schema.sql`.

## DbContext Additions

Add these if your DbContext uses explicit DbSet properties:

```csharp
public DbSet<RecurringIssue> RecurringIssues => Set<RecurringIssue>();
public DbSet<ReleaseChangeLog> ReleaseChangeLogs => Set<ReleaseChangeLog>();
```

## Install Steps

1. Copy all added backend files.
2. Copy the modified backend files or apply equivalent patches manually.
3. Add DbSet properties if your DbContext uses explicit sets.
4. Confirm `modelBuilder.ConfigureEngineeringRequests();` is still called.
5. Confirm `builder.Services.AddEngineeringRequests<ApplicationDbContext>();` is still called.
6. Run:

```powershell
dotnet ef migrations add AddOperationsPack
dotnet ef database update
```

7. Copy all added Vue pages.
8. Copy modified `api.js`, `routes.js`, `RunbookDetails.vue`, and `StatusBadge.vue`.
9. Add navigation links for:
   - Weekly Summary
   - Recurring Issues
   - System Risk
   - Release / Change Log
10. Restart API and Vue app.

## Manual Testing Checklist

Weekly summary:

- Open weekly summary.
- Confirm completed/new/open P1/P2/waiting sections load.
- Click copy summary and paste into a text editor or email draft.

Recurring issues:

- Create a recurring issue.
- Increase recurrence count.
- Mark permanent fix needed.
- Filter by system.

System risk:

- Mark a system High or Critical in Systems Register.
- Add known risks.
- Confirm it appears in the risk dashboard.
- Add recurring issues and confirm repeated issue counts appear.

Release / change log:

- Create a change log linked to a request ID.
- Record files changed, deployment notes, rollback notes, and verified by.
- Filter by system and request ID.

Runbooks:

- Edit a runbook.
- Fill Problem and Fix Steps.
- Confirm LastUpdated changes after save.

UI polish:

- Confirm priority, status, and type badges are readable.
- Confirm empty states appear when no rows exist.
- Confirm forms show required-field browser validation.

## Practical Use

Use the weekly summary for management updates. Use recurring issues and system risk to show technical debt and operational pressure. Use release/change logs to explain what changed and support rollback. Use runbooks to reduce repeated thinking and make future handover less painful.
