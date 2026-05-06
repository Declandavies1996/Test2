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
