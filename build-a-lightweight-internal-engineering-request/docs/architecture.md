# Lightweight Engineering Request Tracking Module

## Recommended Architecture

Use a small vertical feature module inside the existing CAE dashboard.

- Vue pages own the request-tracking screens.
- API controllers expose simple CRUD endpoints.
- Services contain query/update logic.
- EF Core entities map directly to three tables.
- Existing authentication, dashboard shell, layout, logging, and database connection stay unchanged.

Avoid adding queues, notifications, approval states, workflow engines, background jobs, or separate projects for Phase 1.

## Database Schema

### Requests

| Column | Type | Notes |
| --- | --- | --- |
| Id | int identity | Primary key |
| Title | nvarchar(200) | Required |
| Description | nvarchar(4000) | Optional |
| SystemName | nvarchar(120) | Required |
| RequestedBy | nvarchar(120) | Optional |
| Department | nvarchar(120) | Optional |
| Priority | nvarchar(10) | P1, P2, P3, P4 |
| Status | nvarchar(20) | Incoming, Planned, InProgress, Waiting, Done |
| Type | nvarchar(30) | Bug, Feature, Support, Validation, Investigation, TechnicalDebt |
| CreatedDate | datetime2 | UTC recommended |
| UpdatedDate | datetime2 | UTC recommended |
| Notes | nvarchar(4000) | Current summary notes |

### Systems

| Column | Type | Notes |
| --- | --- | --- |
| Id | int identity | Primary key |
| Name | nvarchar(120) | Required, unique |
| Purpose | nvarchar(1000) | Optional |
| MainUsers | nvarchar(500) | Optional |
| Criticality | nvarchar(20) | Low, Medium, High, Critical |
| KnownRisks | nvarchar(2000) | Optional |
| Notes | nvarchar(2000) | Optional |

### RequestNotes

| Column | Type | Notes |
| --- | --- | --- |
| Id | int identity | Primary key |
| RequestId | int | Foreign key to Requests.Id |
| NoteText | nvarchar(4000) | Required |
| CreatedBy | nvarchar(120) | Optional |
| CreatedDate | datetime2 | UTC recommended |

Relationship: one request has many request notes. Delete notes when a request is deleted.

## Backend Models

Files are under:

- `backend/EngineeringRequests/Models`
- `backend/EngineeringRequests/Dtos`
- `backend/EngineeringRequests/Services`
- `backend/EngineeringRequests/Controllers`

Important integration step: replace the generic `TDbContext` controller/service pattern with your actual dashboard DbContext if your ASP.NET Core controller discovery does not support open generic controllers.

Example:

```csharp
public class EngineeringRequestsController : ControllerBase
{
    private readonly EngineeringRequestService<ApplicationDbContext> _service;

    public EngineeringRequestsController(EngineeringRequestService<ApplicationDbContext> service)
    {
        _service = service;
    }
}
```

Register services:

```csharp
builder.Services.AddEngineeringRequests<ApplicationDbContext>();
```

Add entity sets to your DbContext:

```csharp
public DbSet<EngineeringRequest> Requests => Set<EngineeringRequest>();
public DbSet<EngineeringSystem> Systems => Set<EngineeringSystem>();
public DbSet<RequestNote> RequestNotes => Set<RequestNote>();
```

Call the model configuration from `OnModelCreating`:

```csharp
modelBuilder.ConfigureEngineeringRequests();
```

Then run your normal EF Core migration command, for example:

```powershell
dotnet ef migrations add AddEngineeringRequests
dotnet ef database update
```

## CRUD Endpoints

Requests:

- `GET /api/engineering-requests?search=&status=&system=`
- `GET /api/engineering-requests/{id}`
- `POST /api/engineering-requests`
- `PUT /api/engineering-requests/{id}`
- `DELETE /api/engineering-requests/{id}`
- `POST /api/engineering-requests/{id}/notes`
- `GET /api/engineering-requests/summary`

Systems:

- `GET /api/engineering-systems`
- `GET /api/engineering-systems/{id}`
- `POST /api/engineering-systems`
- `PUT /api/engineering-systems/{id}`
- `DELETE /api/engineering-systems/{id}`

## UI Structure

Vue files are under `frontend/engineering-requests`.

- `RequestsList.vue`: searchable/filterable queue and fast request entry.
- `RequestDetails.vue`: request details, status/priority editing, and history notes.
- `SystemsRegister.vue`: supported systems register.
- `EngineeringRequestDashboard.vue`: minimal operational summary.
- `api.js`: API client wrapper.
- `constants.js`: allowed values.
- `routes.js`: route definitions to merge into your existing Vue Router.

Suggested navigation labels:

- Requests
- Request Summary
- Systems Register

## Dashboard Implementation

The summary endpoint returns:

- Open P1 count
- Requests this week
- Open requests grouped by system
- Open requests grouped by type

This is intentionally enough to answer:

- What is on fire?
- Which system is producing the most demand?
- What kind of work is consuming time?
- Has the week become noisy?

## Suggested Folder Structure

```text
src/
  features/
    engineering-requests/
      api.js
      constants.js
      routes.js
      components/
        StatusBadge.vue
        SummaryCards.vue
      pages/
        EngineeringRequestDashboard.vue
        RequestDetails.vue
        RequestsList.vue
        SystemsRegister.vue

Backend/
  EngineeringRequests/
    Models/
    Dtos/
    Services/
    Controllers/
```

Use your actual project conventions if they differ. The important bit is keeping request tracking as one understandable feature area.

## Future Extension Points

Do not implement these in Phase 1, but leave space for them:

- Export request history to CSV.
- Add a simple monthly report page.
- Link requests to documentation pages.
- Add affected release/export name.
- Add estimated effort.
- Add closed date.
- Add tags after you have real usage patterns.
- Add lightweight SLA/reporting only if managers start asking repeat questions.

## Suggested Development Order

1. Add EF Core entities and migration.
2. Seed the 8 supported systems into `Systems`.
3. Add services and API controllers.
4. Smoke test endpoints with Swagger, Postman, or browser dev tools.
5. Add Vue routes and navigation links.
6. Add Systems Register first.
7. Add Requests List with quick creation.
8. Add Request Details and notes.
9. Add summary dashboard.
10. Use it for one week before adding any new fields.

## Practical Recommendations

- Make `Title`, `SystemName`, `Priority`, `Status`, and `Type` the only required fields.
- Treat `Notes` as the current state and `RequestNotes` as the history.
- Keep statuses boring. Most requests should move through `Incoming`, `InProgress`, `Waiting`, and `Done`.
- Use `P1` rarely. If everything is P1, nothing is.
- Add a note when you are interrupted, blocked, or hand context back to yourself.
- Review `Incoming` once daily and `Waiting` twice weekly.
- Do not model every team conversation. Capture decisions, blockers, useful technical facts, and next actions.
- After a month, use the grouped counts to decide where automation or documentation would reduce repeat demand.
