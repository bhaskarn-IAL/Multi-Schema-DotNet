/* =====================================================================
   01  Create the seven schemas in ONE database.
   Run as the database owner BEFORE `dotnet ef database update`.
   (EF also emits CREATE SCHEMA for the schemas it manages, but aud is
    excluded from migrations on SQL Server, so create them all here.)
   ===================================================================== */
IF SCHEMA_ID(N'mrm') IS NULL EXEC(N'CREATE SCHEMA [mrm] AUTHORIZATION [dbo];');
IF SCHEMA_ID(N'sec') IS NULL EXEC(N'CREATE SCHEMA [sec] AUTHORIZATION [dbo];');
IF SCHEMA_ID(N'aud') IS NULL EXEC(N'CREATE SCHEMA [aud] AUTHORIZATION [dbo];');
IF SCHEMA_ID(N'lic') IS NULL EXEC(N'CREATE SCHEMA [lic] AUTHORIZATION [dbo];');
IF SCHEMA_ID(N'app') IS NULL EXEC(N'CREATE SCHEMA [app] AUTHORIZATION [dbo];');
IF SCHEMA_ID(N'dat') IS NULL EXEC(N'CREATE SCHEMA [dat] AUTHORIZATION [dbo];');
IF SCHEMA_ID(N'rpt') IS NULL EXEC(N'CREATE SCHEMA [rpt] AUTHORIZATION [dbo];');
GO

/* Optional: give dat its own filegroup (own storage / restore order).
ALTER DATABASE CURRENT ADD FILEGROUP FG_DAT;
ALTER DATABASE CURRENT ADD FILE (NAME = N'MultiSchema_dat', FILENAME = N'D:\Data\MultiSchema_dat.ndf') TO FILEGROUP FG_DAT;
-- then create/rebuild dat tables ON FG_DAT
*/
