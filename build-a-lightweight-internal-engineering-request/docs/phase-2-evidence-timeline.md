# Phase 2: Request Evidence and Timeline

## Summary of Changes

This phase adds manual operational evidence tracking to the lightweight request module.

- Notes remain attached to requests.
- Attachments are uploaded to disk under `Uploads/Requests/{RequestId}`.
- Attachment metadata is stored in SQL.
- A simple request timeline records creation, notes, uploads, status changes, and priority changes.
- No notifications, approvals, Outlook integration, or complex audit framework.

## Files to Add

- `backend/EngineeringRequests/Models/RequestAttachment.cs`
- `backend/EngineeringRequests/Models/RequestHistory.cs`
- `backend/EngineeringRequests/EngineeringRequestUploadOptions.cs`

## Files to Modify

- `backend/EngineeringRequests/Models/EngineeringRequest.cs`
- `backend/EngineeringRequests/Dtos/EngineeringRequestDtos.cs`
- `backend/EngineeringRequests/EngineeringRequestsDbContextExtensions.cs`
- `backend/EngineeringRequests/Services/EngineeringRequestService.cs`
- `backend/EngineeringRequests/Controllers/EngineeringRequestsController.cs`
- `backend/EngineeringRequests/EngineeringRequestsRegistration.cs`
- `frontend/engineering-requests/api.js`
- `frontend/engineering-requests/pages/RequestDetails.vue`

## DbContext Additions

Add these if your DbContext uses explicit `DbSet` properties:

```csharp
public DbSet<RequestAttachment> RequestAttachments => Set<RequestAttachment>();
public DbSet<RequestHistory> RequestHistory => Set<RequestHistory>();
```

## Program.cs Upload Options

Optional override:

```csharp
builder.Services.Configure<EngineeringRequestUploadOptions>(options =>
{
    options.RootPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads", "Requests");
    options.MaxFileSizeBytes = 20 * 1024 * 1024;
});
```

Keep your existing enum JSON converter:

```csharp
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
```

## Migration SQL

```sql
CREATE TABLE RequestAttachments (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RequestAttachments PRIMARY KEY,
    RequestId int NOT NULL,
    FileName nvarchar(260) NOT NULL,
    StoredFileName nvarchar(260) NOT NULL,
    FilePath nvarchar(1000) NOT NULL,
    ContentType nvarchar(120) NULL,
    UploadedBy nvarchar(120) NULL,
    UploadedDate datetime2 NOT NULL,
    CONSTRAINT FK_RequestAttachments_Requests_RequestId
        FOREIGN KEY (RequestId) REFERENCES Requests(Id) ON DELETE CASCADE
);

CREATE TABLE RequestHistory (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RequestHistory PRIMARY KEY,
    RequestId int NOT NULL,
    ActionType nvarchar(80) NOT NULL,
    OldValue nvarchar(500) NULL,
    NewValue nvarchar(500) NULL,
    ChangedBy nvarchar(120) NULL,
    ChangedDate datetime2 NOT NULL,
    CONSTRAINT FK_RequestHistory_Requests_RequestId
        FOREIGN KEY (RequestId) REFERENCES Requests(Id) ON DELETE CASCADE
);

CREATE INDEX IX_RequestAttachments_RequestId_UploadedDate
ON RequestAttachments(RequestId, UploadedDate);

CREATE INDEX IX_RequestHistory_RequestId_ChangedDate
ON RequestHistory(RequestId, ChangedDate);
```

## Manual Test

1. Start API and Vue app.
2. Open an existing request.
3. Add a note.
4. Confirm the note appears in Notes.
5. Confirm a `NoteAdded` row appears in Timeline.
6. Upload a `.pdf`, `.png`, `.xlsx`, or `.msg` file.
7. Confirm it appears in Attachments.
8. Click the attachment link and verify it downloads/opens.
9. Change status and save.
10. Confirm Timeline shows old and new status.
11. Change priority and save.
12. Confirm Timeline shows old and new priority.
13. Try uploading a blocked file type such as `.exe`.
14. Confirm the API rejects it.

## Practical Use

Use notes for decisions and blockers. Use attachments for proof: emails saved as `.msg`, screenshots, export samples, logs, PDFs, and spreadsheets. The timeline is not a legal-grade audit log; it is operational memory for a solo internal systems developer.
