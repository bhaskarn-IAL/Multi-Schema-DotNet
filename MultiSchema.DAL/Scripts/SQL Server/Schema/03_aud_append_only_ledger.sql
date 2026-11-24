/* =====================================================================
   03  aud tables as APPEND-ONLY LEDGER tables (SQL Server 2022+ / Azure SQL).
   EF cannot create ledger tables, so these are ExcludeFromMigrations()
   in MultiSchemaContext.Aud.cs. Keep columns in sync with Models/Aud.
   UPDATE / DELETE are rejected by the engine for every user, including dbo.
   ===================================================================== */

IF OBJECT_ID(N'aud.AuditTrail', N'U') IS NULL
CREATE TABLE aud.AuditTrail
(
    AuditTrailId   bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditTrail PRIMARY KEY,
    SourceSchema   nvarchar(10)   NOT NULL,
    EntityName     nvarchar(128)  NOT NULL,
    EntityKey      nvarchar(100)  NOT NULL,
    [Action]       nvarchar(20)   NOT NULL,
    OldValuesJson  nvarchar(max)  NULL,
    NewValuesJson  nvarchar(max)  NULL,
    UserId         bigint         NULL,
    CorrelationId  nvarchar(100)  NULL,
    OccurredOn     datetime2      NOT NULL
)
WITH (LEDGER = ON (APPEND_ONLY = ON));
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditTrail_EntityName_EntityKey')
    CREATE INDEX IX_AuditTrail_EntityName_EntityKey ON aud.AuditTrail (EntityName, EntityKey);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditTrail_OccurredOn')
    CREATE INDEX IX_AuditTrail_OccurredOn ON aud.AuditTrail (OccurredOn);
GO

IF OBJECT_ID(N'aud.WorkflowHistory', N'U') IS NULL
CREATE TABLE aud.WorkflowHistory
(
    WorkflowHistoryId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkflowHistory PRIMARY KEY,
    EntityName        nvarchar(128)  NOT NULL,
    EntityKey         nvarchar(100)  NOT NULL,
    FromStatus        nvarchar(20)   NULL,
    ToStatus          nvarchar(20)   NOT NULL,
    ActedByUserId     bigint         NOT NULL,
    Remarks           nvarchar(1000) NULL,
    ActedOn           datetime2      NOT NULL
)
WITH (LEDGER = ON (APPEND_ONLY = ON));
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_WorkflowHistory_EntityName_EntityKey')
    CREATE INDEX IX_WorkflowHistory_EntityName_EntityKey ON aud.WorkflowHistory (EntityName, EntityKey);
GO

IF OBJECT_ID(N'aud.LicenceEvidence', N'U') IS NULL
CREATE TABLE aud.LicenceEvidence
(
    LicenceEvidenceId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_LicenceEvidence PRIMARY KEY,
    EvidenceType      nvarchar(50)   NOT NULL,
    LicenceKey        nvarchar(100)  NULL,
    PayloadJson       nvarchar(max)  NULL,
    CapturedOn        datetime2      NOT NULL
)
WITH (LEDGER = ON (APPEND_ONLY = ON));
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LicenceEvidence_CapturedOn')
    CREATE INDEX IX_LicenceEvidence_CapturedOn ON aud.LicenceEvidence (CapturedOn);
GO

/* Verify ledger integrity periodically:
   EXECUTE sys.sp_verify_database_ledger_from_digest_storage;  -- with automatic digest storage configured */
