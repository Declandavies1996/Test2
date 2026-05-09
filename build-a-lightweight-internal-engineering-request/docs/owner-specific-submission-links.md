# Owner-Specific Submission Links

## Summary of Changes

This enhancement adds owner-specific request submission links.

Each developer/user can open `My Request Submission Link`, copy a tokenized link, and send it to a colleague. Requests submitted through that link are assigned to the link owner and appear in that owner's triage inbox.

No raw Windows username is exposed in the URL.

## Behaviour

Link format:

```text
/Requests/Submit/{ownerToken}
```

When submitted through a token link:

- `OwnerUserName` = link owner
- `RequiresTriage` = true
- `Status` = Incoming
- `Priority` = P3
- `Type` = Investigation
- `SubmittedByUserName` = current Windows user
- `CreatedByUserName` = current Windows user

Triage inbox:

- shows only `RequiresTriage = true`
- shows only `OwnerUserName = current Windows user`

## Files to Add

Backend:

- `backend/EngineeringRequests/Models/SubmissionLink.cs`
- `backend/EngineeringRequests/Dtos/SubmissionLinkDtos.cs`
- `backend/EngineeringRequests/Services/SubmissionLinkService.cs`
- `backend/EngineeringRequests/Controllers/SubmissionLinksController.cs`

Frontend:

- `frontend/engineering-requests/pages/MySubmissionLink.vue`

Docs:

- `docs/owner-specific-submission-links.md`

## Files to Modify

- `backend/EngineeringRequests/EngineeringRequestsDbContextExtensions.cs`
- `backend/EngineeringRequests/EngineeringRequestsRegistration.cs`
- `backend/EngineeringRequests/Controllers/EngineeringRequestsController.cs`
- `backend/EngineeringRequests/Services/EngineeringRequestService.cs`
- `frontend/engineering-requests/api.js`
- `frontend/engineering-requests/routes.js`
- `frontend/engineering-requests/pages/SubmitRequest.vue`
- `docs/sql-schema.sql`

## Database Migration Script

```sql
CREATE TABLE SubmissionLinks (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_SubmissionLinks PRIMARY KEY,
    OwnerUserName nvarchar(256) NOT NULL,
    Token nvarchar(80) NOT NULL,
    DisplayName nvarchar(200) NULL,
    IsActive bit NOT NULL CONSTRAINT DF_SubmissionLinks_IsActive DEFAULT 1,
    CreatedDate datetime2 NOT NULL,
    CreatedByUserName nvarchar(256) NULL,
    CONSTRAINT UX_SubmissionLinks_Token UNIQUE (Token)
);

CREATE INDEX IX_SubmissionLinks_OwnerUserName ON SubmissionLinks(OwnerUserName);
CREATE INDEX IX_SubmissionLinks_IsActive ON SubmissionLinks(IsActive);
```

## New API Endpoints

- `GET /api/submission-links/mine`
- `GET /api/submission-links/{token}`

Updated:

- `POST /api/engineering-requests/submit`
  - accepts optional form field `ownerToken`

## Setup Steps

1. Copy added backend files.
2. Copy modified backend files.
3. Add DbSet if your DbContext uses explicit sets:

```csharp
public DbSet<SubmissionLink> SubmissionLinks => Set<SubmissionLink>();
```

4. Run:

```powershell
dotnet ef migrations add AddOwnerSubmissionLinks
dotnet ef database update
```

5. Copy frontend changes.
6. Add navigation link for `My Request Submission Link`.
7. Restart API and Vue app.

## Manual Test Checklist

1. Log in as developer A.
2. Open `/requests/my-submission-link`.
3. Copy the generated `/Requests/Submit/{token}` link.
4. Open that link as a colleague.
5. Submit a request with an attachment.
6. Confirm the request has:
   - `OwnerUserName = developer A`
   - `SubmittedByUserName = colleague`
   - `RequiresTriage = true`
   - `Status = Incoming`
7. Log in as developer A.
8. Open `/Requests/Triage`.
9. Confirm the request appears.
10. Log in as developer B.
11. Confirm developer A's request does not appear in developer B's triage inbox.
