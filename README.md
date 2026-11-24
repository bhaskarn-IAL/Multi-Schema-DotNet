# DotNet-MultiSchema

A .NET 10 Web API whose DAL puts **seven schemas in one database behind one `DbContext`**, with the same layering as ECL3. It supports SQL Server, PostgreSQL, Oracle and MySQL.

```
MultiSchema.sln
├─ MultiSchema.Common.Constant   enDBInstance, enMySqlSchemaMode, CommonConts, DbSchema (mrm/sec/aud/lic/app/dat/rpt)
├─ MultiSchema.Common.EntityBase IAudit, IMakerChecker, IAppendOnly (aud), IAppReadOnly (lic), IProjection (rpt)
├─ MultiSchema.DAL.Base          DBConnection (ECL3 naming: "", MYSQL-, PGSQL-, ORACLE- + MULTISCHEMADBCONTEXT)
├─ MultiSchema.DAL
│   ├─ Data/MultiSchemaContext.cs          the one context + DbSets per schema
│   ├─ Data/MultiSchemaContext.<Schema>.cs per-schema fluent config (Mrm, Sec, Aud, Lic, App, Dat, Rpt)
│   ├─ Data/DbProvider.cs                  provider switch (like ECL3Context.OnConfiguring)
│   ├─ Data/Conventions/SchemaConventions  logical -> physical schema per provider
│   ├─ Data/Interceptors/SchemaWriteGuard  blocks aud update/delete and any lic write
│   ├─ Models/<Schema>/*.cs                entities, [Table("X", Schema = DbSchema.Mrm)]
│   └─ Scripts/<Provider>/Schema/*.sql     schemas/users, roles, grants, append-only enforcement
├─ MultiSchema.BLL.Common        Signature/ + Implementation/ (cross-schema read & transactional write sample)
└─ MultiSchema.API               Program.cs, Controllers (Schema diagnostics, ModelInventory)
```

## Can one DbContext span the seven schemas?

Yes. In EF Core a table is identified by its schema and table name. Every entity declares its schema, and one connection reaches all seven schemas. That gives you:

- Cross-schema joins and FKs, e.g. `mrm.ModelRecords` → `sec.Users`.
- One local transaction across schemas, e.g. `mrm` + `aud` written atomically, with no distributed transaction (DTC).

The isolation the schema plan asks for comes from **database permissions per schema**, not from separate contexts. The scripts set this up. `SchemaWriteGuardInterceptor` is an in-app second check.

| Provider | Physical form of logical `mrm.ModelRecords` | Notes |
|---|---|---|
| SQL Server | `[mrm].[ModelRecords]` | `mrm` tables are temporal (system-versioned). `aud` tables are APPEND_ONLY ledger tables created by script. |
| PostgreSQL | `mrm."ModelRecords"` | Owner and app are separate roles. Triggers enforce append-only on `aud`. |
| Oracle | `"MRM"."ModelRecords"` | A schema is a user. Schema-only users come from the script. 19c needs per-table grants after each migration. |
| MySQL – Database mode (default) | `` `mrm`.`ModelRecords` `` | A schema is a database on the same server. Real per-schema grants. |
| MySQL – Prefix mode | `` `mrm_ModelRecords` `` | Use only if you are limited to a single database. Grants are per table. |

Entities never change between providers. `SchemaConventions.Apply` rewrites the schema and table names as the last step of `OnModelCreating`.

## Per-schema permissions (scripts)

| Schema | app runtime login | other parties |
|---|---|---|
| mrm, app, dat | SELECT/INSERT/UPDATE/DELETE | — |
| sec | SELECT/INSERT/UPDATE/DELETE | `mm_iam_reader`: SELECT |
| aud | SELECT, INSERT only (UPDATE/DELETE denied, plus ledger or trigger) | `mm_auditor`: SELECT |
| lic | SELECT only | `mm_lic_admin`: full |
| rpt | SELECT/INSERT/UPDATE/DELETE (rebuilds) | `mm_bi_reader`: SELECT (the only schema BI can see) |

Use two logins: a **migrator** that owns the schemas and runs `dotnet ef`, and the **app** login that goes in `ConnectionStrings`. Grants do not apply to an object's owner, so the app must never connect as the owner.

## Run

```powershell
# appsettings.json -> Database:Provider = SqlServer | PostgreSQL | MySql | Oracle
dotnet run --project MultiSchema.API
# GET /api/schema/map   entity -> schema.table for the active provider (no DB needed)
# GET /api/schema/ddl   CREATE script for the active provider (Development only)
# GET /api/schema/health  row count per schema over one connection
```

Override the provider without editing files with `$env:Database__Provider="Oracle"`.

## Migrations

Migrations are provider-specific. Generate them per provider, ideally in a separate migrations project for each provider:

```powershell
$env:MULTISCHEMA_PROVIDER="SqlServer"; $env:MULTISCHEMA_CONNECTION="<migrator login>"
dotnet ef migrations add Init --project MultiSchema.DAL --output-dir Migrations/SqlServer
```

Order of operations: run `Scripts/<Provider>/Schema/01_*`, then `dotnet ef database update`, then the remaining scripts (grants, ledger or triggers). `__EFMigrationsHistory` lives in `app`.
