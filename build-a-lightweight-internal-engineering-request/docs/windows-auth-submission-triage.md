# Windows Auth Submission and Triage Enhancement

## Summary of Changes

This enhancement makes the request module user-aware using the existing Windows Authentication identity.

Added:

- Request ownership/user audit fields.
- My Requests default filtering.
- My Dashboard / My Weekly Summary default filtering.
- Colleague submission page at `/Requests/Submit`.
- Submission attachments using the existing disk-backed attachment system.
- Triage API and page at `/Requests/Triage`.
- My submitted requests page at `/Requests/MySubmitted`.
- Reusable request categorisation guidance panel.

No new identity system, permissions framework, approvals, notifications, email integration, or Outlook API integration was added.

## Files to Add

Frontend:

- `frontend/engineering-requests/components/RequestGuidance.vue`
- `frontend/engineering-requests/pages/SubmitRequest.vue`
- `frontend/engineering-requests/pages/TriageRequests.vue`
- `frontend/engineering-requests/pages/MySubmittedRequests.vue`

Docs:

- `docs/windows-auth-submission-triage.md`

## Files to Modify

Backend:

- `backend/EngineeringRequests/Models/EngineeringRequest.cs`
- `backend/EngineeringRequests/Dtos/EngineeringRequestDtos.cs`
- `backend/EngineeringRequests/EngineeringRequestsDbContextExtensions.cs`
- `backend/EngineeringRequests/Services/EngineeringRequestService.cs`
- `backend/EngineeringRequests/Controllers/EngineeringRequestsController.cs`

Frontend:

- `frontend/engineering-requests/api.js`
- `frontend/engineering-requests/routes.js`
- `frontend/engineering-requests/pages/RequestsList.vue`
- `frontend/engineering-requests/pages/RequestDetails.vue`
- `frontend/engineering-requests/pages/EngineeringRequestDashboard.vue`
- `frontend/engineering-requests/pages/WeeklyManagementSummary.vue`

SQL:

- `docs/sql-schema.sql`

## Database Migration Script

```sql
ALTER TABLE Requests ADD OwnerUserName nvarchar(256) NULL;
ALTER TABLE Requests ADD CreatedByUserName nvarchar(256) NULL;
ALTER TABLE Requests ADD UpdatedByUserName nvarchar(256) NULL;
ALTER TABLE Requests ADD SubmittedByUserName nvarchar(256) NULL;
ALTER TABLE Requests ADD TriagedByUserName nvarchar(256) NULL;
ALTER TABLE Requests ADD TriagedDate datetime2 NULL;
ALTER TABLE Requests ADD IsUserSubmitted bit NOT NULL CONSTRAINT DF_Requests_IsUserSubmitted DEFAULT 0;
ALTER TABLE Requests ADD RequiresTriage bit NOT NULL CONSTRAINT DF_Requests_RequiresTriage DEFAULT 0;
ALTER TABLE Requests ADD UrgencyExplanation nvarchar(2000) NULL;
ALTER TABLE Requests ADD BusinessReason nvarchar(4000) NULL;
ALTER TABLE Requests ADD ExpectedBehaviour nvarchar(4000) NULL;
ALTER TABLE Requests ADD ActualBehaviour nvarchar(4000) NULL;

UPDATE Requests
SET
    OwnerUserName = COALESCE(OwnerUserName, RequestedBy, CreatedByUserName, 'Unassigned'),
    CreatedByUserName = COALESCE(CreatedByUserName, RequestedBy, 'Unknown'),
    UpdatedByUserName = COALESCE(UpdatedByUserName, RequestedBy, 'Unknown')
WHERE OwnerUserName IS NULL OR CreatedByUserName IS NULL OR UpdatedByUserName IS NULL;

CREATE INDEX IX_Requests_OwnerUserName ON Requests(OwnerUserName);
CREATE INDEX IX_Requests_SubmittedByUserName ON Requests(SubmittedByUserName);
CREATE INDEX IX_Requests_RequiresTriage ON Requests(RequiresTriage);
```

## New API Endpoints

- `POST /api/engineering-requests/submit`
- `GET /api/engineering-requests/my-submitted`
- `GET /api/engineering-requests/triage`
- `POST /api/engineering-requests/{id}/triage`

Updated:

- `GET /api/engineering-requests?allRequests=true`
- `GET /api/engineering-requests/reporting?allRequests=true`
- `GET /api/engineering-requests/weekly-management-summary?allRequests=true`

Default behavior is user-specific. Pass `allRequests=true` only for the broader operational view.

## Windows Authentication Notes

Controllers use:

```csharp
User?.Identity?.Name
```

Stored automatically:

- `CreatedByUserName`
- `UpdatedByUserName`
- `SubmittedByUserName`
- `TriagedByUserName`
- note `CreatedBy`
- attachment `UploadedBy`
- history `ChangedBy`

## Setup Instructions

1. Copy backend changes.
2. Run EF migration:

```powershell
dotnet ef migrations add AddWindowsAuthSubmissionTriage
dotnet ef database update
```

3. Copy frontend pages, component, route changes, and API changes.
4. Add navigation links:
   - Submit Request
   - My Submitted Requests
   - Request Triage
5. Restart API and Vue app.

## Manual Test Checklist

Submission:

- Open `/Requests/Submit`.
- Submit a request without setting priority/status/type.
- Upload an allowed attachment.
- Confirm request is created as `Incoming`, `P3`, `Investigation`.
- Confirm `RequiresTriage = true`.
- Confirm attachment exists under `/Uploads/Requests/{RequestId}/`.

Triage:

- Open `/Requests/Triage`.
- Set system, priority, status, type, owner, and notes.
- Confirm request disappears from triage page.
- Confirm timeline shows triage, priority/system/type/owner/status changes.

User-specific behavior:

- Open requests list as a user and confirm it defaults to that user's requests.
- Tick `All requests` and confirm broader list appears.
- Open dashboard and weekly summary and confirm default is user-specific.

Guidance:

- Confirm guidance panel appears on submit, create/list, details, and triage screens.

Attachments/notes:

- Add a note and confirm `CreatedBy` is current Windows user.
- Upload an attachment and confirm `UploadedBy` is current Windows user.
