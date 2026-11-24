/* =====================================================================
   02  Roles and per-schema permissions.
   Schema-level GRANT/DENY automatically covers tables added later by migrations.

   Logins (create at server level, then map with CREATE USER ... FOR LOGIN):
     mm_migrator_login  -> runs EF migrations (member of db_owner, NOT used at runtime)
     mm_app_login       -> the API's runtime connection string (MULTISCHEMADBCONTEXT)
     mm_iam_login       -> client IAM / audit team (user-access recertification)
     mm_licadmin_login  -> client sysadmin who installs licences
     mm_bi_login        -> client BI tools
   ===================================================================== */

CREATE ROLE mm_app;        -- the application
CREATE ROLE mm_iam_reader; -- sec read-only
CREATE ROLE mm_auditor;    -- aud read-only
CREATE ROLE mm_lic_admin;  -- lic writer
CREATE ROLE mm_bi_reader;  -- rpt read-only
GO

/* ---- mm_app ------------------------------------------------------- */
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::mrm TO mm_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::sec TO mm_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::app TO mm_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dat TO mm_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::rpt TO mm_app;

-- aud: append-only
GRANT SELECT, INSERT ON SCHEMA::aud TO mm_app;
DENY  UPDATE, DELETE ON SCHEMA::aud TO mm_app;

-- lic: read-only for the app
GRANT SELECT ON SCHEMA::lic TO mm_app;
DENY  INSERT, UPDATE, DELETE ON SCHEMA::lic TO mm_app;
GO

/* ---- other parties ------------------------------------------------ */
GRANT SELECT ON SCHEMA::sec TO mm_iam_reader;                   -- no business data
GRANT SELECT ON SCHEMA::aud TO mm_auditor;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::lic TO mm_lic_admin;
GRANT SELECT ON SCHEMA::rpt TO mm_bi_reader;                    -- the only schema BI can read
GO

/* ---- map users ---------------------------------------------------- */
-- CREATE USER mm_migrator FOR LOGIN mm_migrator_login; ALTER ROLE db_owner     ADD MEMBER mm_migrator;
-- CREATE USER mm_app      FOR LOGIN mm_app_login;      ALTER ROLE mm_app       ADD MEMBER mm_app;
-- CREATE USER mm_iam      FOR LOGIN mm_iam_login;      ALTER ROLE mm_iam_reader ADD MEMBER mm_iam;
-- CREATE USER mm_licadmin FOR LOGIN mm_licadmin_login; ALTER ROLE mm_lic_admin ADD MEMBER mm_licadmin;
-- CREATE USER mm_bi       FOR LOGIN mm_bi_login;       ALTER ROLE mm_bi_reader ADD MEMBER mm_bi;
