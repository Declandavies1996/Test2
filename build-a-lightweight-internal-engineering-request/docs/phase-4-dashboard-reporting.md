# Phase 4: Dashboard Reporting

## Summary of Changes

This phase upgrades the simple request summary into a practical operational reporting dashboard.

It adds:

- Reporting cards for urgent, open, weekly, and blocked work.
- Open requests by system table.
- Requests by type table.
- Oldest open requests table.
- Waiting requests table with latest note as the blocker reason when available.
- Filters for date range, system, priority, status, and type.

No new database tables are required. This phase uses existing request and request note data.

## Backend Model / View Model

Added DTOs in `backend/EngineeringRequests/Dtos/EngineeringRequestDtos.cs`:

- `RequestReportingFilterDto`
- `RequestReportingDashboardDto`
- `RequestReportingCardsDto`
- `OpenRequestsBySystemDto`
- `RequestsByTypeDto`
- `OldestOpenRequestDto`
- `WaitingRequestDto`

## Query / Service Logic

Added service method in `backend/EngineeringRequests/Services/EngineeringRequestService.cs`:

```csharp
GetReportingDashboardAsync(RequestReportingFilterDto filters, CancellationToken cancellationToken)
```

Rules:

- Open requests are requests where `Status != Done`.
- Open P1/P2 cards count only open requests.
- Created this week uses `CreatedDate`.
- Completed this week uses `Status == Done` and `UpdatedDate`.
- Waiting / blocked uses `Status == Waiting`.
- Waiting reason uses the latest `RequestNote.NoteText` if available.
- Oldest open requests are ordered by `CreatedDate`.

## Controller Changes

Added endpoint in `backend/EngineeringRequests/Controllers/EngineeringRequestsController.cs`:

```text
GET /api/engineering-requests/reporting
```

Supported query parameters:

- `fromDate`
- `toDate`
- `system`
- `priority`
- `status`
- `type`

Example:

```text
/api/engineering-requests/reporting?system=Harness Export&priority=P1
```

## Dashboard UI

Replaced `frontend/engineering-requests/pages/EngineeringRequestDashboard.vue` with a simple reporting page.

Dashboard cards:

- Open P1
- Open P2
- Total open
- Created this week
- Completed this week
- Waiting / blocked

Tables:

- Open Requests by System
- Requests by Type
- Oldest Open Requests
- Waiting Requests

Rows in oldest/waiting request tables navigate to request details.

## Files to Add

- `docs/phase-4-dashboard-reporting.md`

## Files to Modify

- `backend/EngineeringRequests/Dtos/EngineeringRequestDtos.cs`
- `backend/EngineeringRequests/Services/EngineeringRequestService.cs`
- `backend/EngineeringRequests/Controllers/EngineeringRequestsController.cs`
- `frontend/engineering-requests/api.js`
- `frontend/engineering-requests/pages/EngineeringRequestDashboard.vue`

## Database / Migration SQL

No migration is required for Phase 4.

Optional indexes if the database grows and the dashboard feels slow:

```sql
CREATE INDEX IX_Requests_Status_Priority ON Requests(Status, Priority);
CREATE INDEX IX_Requests_Status_SystemName ON Requests(Status, SystemName);
CREATE INDEX IX_Requests_Status_CreatedDate ON Requests(Status, CreatedDate);
CREATE INDEX IX_Requests_Type ON Requests(Type);
```

Do not add these until you have enough data to justify them. The existing indexes from earlier phases are likely enough for a lightweight internal tool.

## Install Steps

1. Copy the modified backend DTO file.
2. Copy the modified request service file.
3. Copy the modified request controller file.
4. Copy the modified `api.js`.
5. Replace the existing dashboard page with the new `EngineeringRequestDashboard.vue`.
6. Keep your existing route to `/engineering-requests-summary`, or rename the navigation label to `Reporting`.
7. Restart the API.
8. Restart the Vue app.

## Manual Testing Checklist

1. Open the reporting page.
2. Confirm all six cards load.
3. Create or identify one open `P1` request and confirm Open P1 increments.
4. Create or identify one open `P2` request and confirm Open P2 increments.
5. Mark a request as `Waiting` and confirm Waiting / blocked increments.
6. Add a note to the waiting request and confirm it appears in the Waiting Requests reason column.
7. Mark a request as `Done` and confirm Completed this week increments.
8. Confirm Open Requests by System groups open requests correctly.
9. Confirm Requests by Type shows all six request types, including zero counts.
10. Confirm Oldest Open Requests are ordered oldest first.
11. Test filters one at a time:
    - Date range
    - System
    - Priority
    - Status
    - Type
12. Click a row in Oldest Open Requests and confirm it opens request details.
13. Click a row in Waiting Requests and confirm it opens request details.

## Practical Use

Use this dashboard for weekly updates and quick self-triage:

- P1/P2 cards show immediate operational risk.
- Waiting table explains blockers and delays.
- Oldest open table shows backlog pressure.
- Open by system shows where repeat demand is coming from.
- Type breakdown shows whether your time is going into bugs, support, validation, exports, or technical debt.
