CREATE TABLE Systems (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Systems PRIMARY KEY,
    Name nvarchar(120) NOT NULL,
    Purpose nvarchar(1000) NULL,
    MainUsers nvarchar(500) NULL,
    Criticality nvarchar(20) NOT NULL,
    KnownRisks nvarchar(2000) NULL,
    Notes nvarchar(2000) NULL,
    CONSTRAINT UX_Systems_Name UNIQUE (Name)
);

CREATE TABLE Requests (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Requests PRIMARY KEY,
    Title nvarchar(200) NOT NULL,
    Description nvarchar(4000) NULL,
    SystemName nvarchar(120) NOT NULL,
    RequestedBy nvarchar(120) NULL,
    Department nvarchar(120) NULL,
    Priority nvarchar(10) NOT NULL,
    Status nvarchar(20) NOT NULL,
    Type nvarchar(30) NOT NULL,
    CreatedDate datetime2 NOT NULL,
    UpdatedDate datetime2 NOT NULL,
    Notes nvarchar(4000) NULL
);

CREATE TABLE RequestNotes (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RequestNotes PRIMARY KEY,
    RequestId int NOT NULL,
    NoteText nvarchar(4000) NOT NULL,
    CreatedBy nvarchar(120) NULL,
    CreatedDate datetime2 NOT NULL,
    CONSTRAINT FK_RequestNotes_Requests_RequestId
        FOREIGN KEY (RequestId) REFERENCES Requests(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Requests_Status ON Requests(Status);
CREATE INDEX IX_Requests_Priority ON Requests(Priority);
CREATE INDEX IX_Requests_SystemName ON Requests(SystemName);
CREATE INDEX IX_Requests_CreatedDate ON Requests(CreatedDate);
CREATE INDEX IX_RequestNotes_RequestId_CreatedDate ON RequestNotes(RequestId, CreatedDate);

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

CREATE TABLE RecurringIssues (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RecurringIssues PRIMARY KEY,
    SystemName nvarchar(120) NOT NULL,
    IssueSummary nvarchar(500) NOT NULL,
    RecurrenceCount int NOT NULL,
    TemporaryFix nvarchar(4000) NULL,
    SuspectedRootCause nvarchar(4000) NULL,
    PermanentFixNeeded bit NOT NULL,
    RelatedRequestIds nvarchar(1000) NULL,
    CreatedDate datetime2 NOT NULL,
    UpdatedDate datetime2 NOT NULL
);

CREATE TABLE ReleaseChangeLogs (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReleaseChangeLogs PRIMARY KEY,
    RequestId int NULL,
    SystemName nvarchar(120) NOT NULL,
    ReleaseDate datetime2 NOT NULL,
    Summary nvarchar(1000) NOT NULL,
    FilesChanged nvarchar(4000) NULL,
    DeploymentNotes nvarchar(4000) NULL,
    RollbackNotes nvarchar(4000) NULL,
    VerifiedBy nvarchar(120) NULL,
    CONSTRAINT FK_ReleaseChangeLogs_Requests_RequestId
        FOREIGN KEY (RequestId) REFERENCES Requests(Id) ON DELETE SET NULL
);

CREATE INDEX IX_RecurringIssues_SystemName ON RecurringIssues(SystemName);
CREATE INDEX IX_RecurringIssues_PermanentFixNeeded ON RecurringIssues(PermanentFixNeeded);
CREATE INDEX IX_ReleaseChangeLogs_SystemName ON ReleaseChangeLogs(SystemName);
CREATE INDEX IX_ReleaseChangeLogs_ReleaseDate ON ReleaseChangeLogs(ReleaseDate);
CREATE INDEX IX_ReleaseChangeLogs_RequestId ON ReleaseChangeLogs(RequestId);

ALTER TABLE Runbooks ADD Problem nvarchar(4000) NULL;
ALTER TABLE Runbooks ADD FixSteps nvarchar(8000) NULL;
ALTER TABLE Runbooks ADD LastUpdated datetime2 NOT NULL CONSTRAINT DF_Runbooks_LastUpdated DEFAULT SYSUTCDATETIME();

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
