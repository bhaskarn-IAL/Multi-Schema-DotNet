-- =====================================================================
-- Oracle: a "schema" is a user. Create seven schema-only users (no login)
-- in ONE database/PDB, plus the migrator and runtime app accounts.
-- EF maps logical mrm -> "MRM" (see SchemaConventions), so names are upper case.
-- Run as a DBA BEFORE `dotnet ef database update`.
-- NO AUTHENTICATION requires 18c+; on older versions use IDENTIFIED BY + ACCOUNT LOCK.
-- =====================================================================

CREATE USER MRM NO AUTHENTICATION DEFAULT TABLESPACE USERS QUOTA UNLIMITED ON USERS;
CREATE USER SEC NO AUTHENTICATION DEFAULT TABLESPACE USERS QUOTA UNLIMITED ON USERS;
CREATE USER AUD NO AUTHENTICATION DEFAULT TABLESPACE USERS QUOTA UNLIMITED ON USERS;
CREATE USER LIC NO AUTHENTICATION DEFAULT TABLESPACE USERS QUOTA UNLIMITED ON USERS;
CREATE USER APP NO AUTHENTICATION DEFAULT TABLESPACE USERS QUOTA UNLIMITED ON USERS;
CREATE USER DAT NO AUTHENTICATION DEFAULT TABLESPACE USERS QUOTA UNLIMITED ON USERS;   -- consider its own tablespace
CREATE USER RPT NO AUTHENTICATION DEFAULT TABLESPACE USERS QUOTA UNLIMITED ON USERS;

-- Migrator: creates objects in the other schemas (dev/CI; in prod a DBA can run the generated script instead)
CREATE USER MM_MIGRATOR IDENTIFIED BY "change-me";
GRANT CREATE SESSION TO MM_MIGRATOR;
GRANT CREATE ANY TABLE, ALTER ANY TABLE, DROP ANY TABLE, CREATE ANY INDEX, DROP ANY INDEX,
      CREATE ANY SEQUENCE, DROP ANY SEQUENCE, CREATE ANY TRIGGER, DROP ANY TRIGGER,
      SELECT ANY TABLE, INSERT ANY TABLE, UPDATE ANY TABLE, DELETE ANY TABLE
      TO MM_MIGRATOR;
-- Cross-schema FKs (MRM -> SEC) need object-level REFERENCES granted to the FK-owning schema.
-- Run between the migration that creates SEC."Users" and the one that adds the FK, or create
-- SEC tables first: GRANT REFERENCES ON "SEC"."Users" TO MRM;  (also in 02_grants.sql)

-- Runtime app login (MULTISCHEMADBCONTEXT) + roles
CREATE USER MM_APP IDENTIFIED BY "change-me";
GRANT CREATE SESSION TO MM_APP;

CREATE ROLE MM_APP_ROLE;
CREATE ROLE MM_IAM_READER;
CREATE ROLE MM_AUDITOR;
CREATE ROLE MM_LIC_ADMIN;
CREATE ROLE MM_BI_READER;
GRANT MM_APP_ROLE TO MM_APP;
