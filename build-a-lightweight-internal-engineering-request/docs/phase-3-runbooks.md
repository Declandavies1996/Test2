# Phase 3: Runbooks / Fix Guides

## Summary of Changes

This phase adds a simple searchable runbook system to the CAE dashboard request tracker.

- Create and update runbooks manually.
- Search runbooks by title, system, category, symptoms, cause, and resolution steps.
- View runbook details.
- Link one or more runbooks to a request from the request details page.
- Record link and unlink actions in the request timeline.

This is intentionally small. It is operational memory for repeated fixes, not a full knowledge-base platform.

## Database Changes

New tables:

- `Runbooks`
- `RequestRunbooks`

New enum:

- `RunbookCategory`

Categories:

- `Deployment`
- `Validation`
- `DataFix`
- `ImportExport`
- `Infrastructure`
- `Troubleshooting`
- `Other`

## Files to Add

- `backend/EngineeringRequests/Models/Runbook.cs`
- `backend/EngineeringRequests/Models/RequestRunbook.cs`
- `backend/EngineeringRequests/Models/RunbookCategory.cs`
- `backend/EngineeringRequests/Dtos/RunbookDtos.cs`
- `backend/EngineeringRequests/Services/RunbookService.cs`
- `backend/EngineeringRequests/Controllers/RunbooksController.cs`
- `frontend/engineering-requests/pages/RunbooksList.vue`
- `frontend/engineering-requests/pages/RunbookDetails.vue`

## Files to Modify

- `backend/EngineeringRequests/Models/EngineeringRequest.cs`
- `backend/EngineeringRequests/Dtos/EngineeringRequestDtos.cs`
- `backend/EngineeringRequests/EngineeringRequestsDbContextExtensions.cs`
- `backend/EngineeringRequests/Services/EngineeringRequestService.cs`
- `backend/EngineeringRequests/Controllers/EngineeringRequestsController.cs`
- `backend/EngineeringRequests/EngineeringRequestsRegistration.cs`
- `frontend/engineering-requests/api.js`
- `frontend/engineering-requests/constants.js`
- `frontend/engineering-requests/routes.js`
- `frontend/engineering-requests/pages/RequestDetails.vue`
- `docs/sql-schema.sql`

## DbContext Additions

Add these if your DbContext uses explicit `DbSet` properties:

```csharp
public DbSet<Runbook> Runbooks => Set<Runbook>();
public DbSet<RequestRunbook> RequestRunbooks => Set<RequestRunbook>();
```

## Controller Note

The portable pack uses generic controller templates. In your real app, use your actual DbContext name in non-generic controllers if ASP.NET Core does not discover the generic controllers.

Example:

```csharp
public class RunbooksController : ControllerBase
{
    private readonly RunbookService<ApplicationDbContext> _service;

    public RunbooksController(RunbookService<ApplicationDbContext> service)
    {
        _service = service;
    }
}
```

## API Endpoints

Runbooks:

- `GET /api/runbooks?search=&system=&category=`
- `GET /api/runbooks/{id}`
- `POST /api/runbooks`
- `PUT /api/runbooks/{id}`
- `DELETE /api/runbooks/{id}`

Request links:

- `POST /api/engineering-requests/{id}/runbooks`
- `DELETE /api/engineering-requests/{id}/runbooks/{runbookId}`

## Migration SQL

```sql
CREATE TABLE Runbooks (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Runbooks PRIMARY KEY,
    Title nvarchar(200) NOT NULL,
    SystemName nvarchar(120) NOT NULL,
    Category nvarchar(30) NOT NULL,
    Symptoms nvarchar(4000) NULL,
    Cause nvarchar(4000) NULL,
    ResolutionSteps nvarchar(8000) NULL,
    VerificationSteps nvarchar(4000) NULL,
    KnownRisks nvarchar(4000) NULL,
    Notes nvarchar(4000) NULL,
    CreatedDate datetime2 NOT NULL,
    UpdatedDate datetime2 NOT NULL
);

CREATE TABLE RequestRunbooks (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RequestRunbooks PRIMARY KEY,
    RequestId int NOT NULL,
    RunbookId int NOT NULL,
    LinkedDate datetime2 NOT NULL,
    CONSTRAINT FK_RequestRunbooks_Requests_RequestId
        FOREIGN KEY (RequestId) REFERENCES Requests(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RequestRunbooks_Runbooks_RunbookId
        FOREIGN KEY (RunbookId) REFERENCES Runbooks(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Runbooks_SystemName ON Runbooks(SystemName);
CREATE INDEX IX_Runbooks_Category ON Runbooks(Category);
CREATE INDEX IX_Runbooks_UpdatedDate ON Runbooks(UpdatedDate);
CREATE UNIQUE INDEX UX_RequestRunbooks_RequestId_RunbookId
ON RequestRunbooks(RequestId, RunbookId);
CREATE INDEX IX_RequestRunbooks_RunbookId ON RequestRunbooks(RunbookId);
```

## Install Steps

1. Copy the added backend model, DTO, service, and controller files.
2. Copy the modified backend files or apply the same patches manually.
3. Add the two DbSet properties if your DbContext uses them.
4. Confirm `modelBuilder.ConfigureEngineeringRequests();` is still called.
5. Confirm `builder.Services.AddEngineeringRequests<ApplicationDbContext>();` is still called.
6. Run:

```powershell
dotnet ef migrations add AddRunbooks
dotnet ef database update
```

7. Copy the new Vue pages.
8. Copy the modified `api.js`, `constants.js`, `routes.js`, and `RequestDetails.vue`.
9. Add navigation links in your dashboard shell for:
   - `Runbooks`
   - optionally `New Runbook`
10. Restart the API and Vue app.

## Manual Test Steps

1. Open `/runbooks`.
2. Create a runbook for one system.
3. Confirm it appears in the runbooks list.
4. Search by part of the title.
5. Filter by system.
6. Filter by category.
7. Open the runbook details page.
8. Edit resolution steps and save.
9. Open an existing request details page.
10. Link the runbook to the request.
11. Confirm the linked runbook appears on the request.
12. Confirm the request timeline shows `Runbook linked`.
13. Click the linked runbook and confirm it opens.
14. Unlink the runbook.
15. Confirm the request timeline shows `Runbook unlinked`.

## Practical Use

Good runbooks are short and repeatable. Capture symptoms, likely cause, exact fix steps, and verification steps. Do not try to document every historical detail; keep evidence on the request and keep the runbook focused on repeatable recovery.
