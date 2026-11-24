-- =====================================================================
-- PostgreSQL: seven schemas in ONE database, roles and grants.
-- Run as a superuser / database owner BEFORE `dotnet ef database update`.
-- Tables are created by mm_migrator (the owner). The app connects as
-- mm_app, which is NOT the owner, so grants actually bind it.
-- =====================================================================

-- Roles (NOLOGIN groups) + logins
CREATE ROLE mm_migrator   LOGIN PASSWORD 'change-me';
CREATE ROLE mm_app        LOGIN PASSWORD 'change-me';
CREATE ROLE mm_iam_reader NOLOGIN;
CREATE ROLE mm_auditor    NOLOGIN;
CREATE ROLE mm_lic_admin  NOLOGIN;
CREATE ROLE mm_bi_reader  NOLOGIN;

CREATE SCHEMA IF NOT EXISTS mrm AUTHORIZATION mm_migrator;
CREATE SCHEMA IF NOT EXISTS sec AUTHORIZATION mm_migrator;
CREATE SCHEMA IF NOT EXISTS aud AUTHORIZATION mm_migrator;
CREATE SCHEMA IF NOT EXISTS lic AUTHORIZATION mm_migrator;
CREATE SCHEMA IF NOT EXISTS app AUTHORIZATION mm_migrator;
CREATE SCHEMA IF NOT EXISTS dat AUTHORIZATION mm_migrator;
CREATE SCHEMA IF NOT EXISTS rpt AUTHORIZATION mm_migrator;

-- Nobody else gets the public schema
REVOKE ALL ON SCHEMA public FROM PUBLIC;

-- ---- mm_app --------------------------------------------------------
GRANT USAGE ON SCHEMA mrm, sec, aud, lic, app, dat, rpt TO mm_app;

-- Default privileges cover tables the migrator creates LATER.
ALTER DEFAULT PRIVILEGES FOR ROLE mm_migrator IN SCHEMA mrm, sec, app, dat, rpt
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO mm_app;
ALTER DEFAULT PRIVILEGES FOR ROLE mm_migrator IN SCHEMA mrm, sec, app, dat, rpt, aud
    GRANT USAGE, SELECT ON SEQUENCES TO mm_app;               -- identity columns
ALTER DEFAULT PRIVILEGES FOR ROLE mm_migrator IN SCHEMA aud
    GRANT SELECT, INSERT ON TABLES TO mm_app;                  -- append-only
ALTER DEFAULT PRIVILEGES FOR ROLE mm_migrator IN SCHEMA lic
    GRANT SELECT ON TABLES TO mm_app;                          -- read-only

-- ---- other parties -------------------------------------------------
GRANT USAGE ON SCHEMA sec TO mm_iam_reader;
ALTER DEFAULT PRIVILEGES FOR ROLE mm_migrator IN SCHEMA sec GRANT SELECT ON TABLES TO mm_iam_reader;

GRANT USAGE ON SCHEMA aud TO mm_auditor;
ALTER DEFAULT PRIVILEGES FOR ROLE mm_migrator IN SCHEMA aud GRANT SELECT ON TABLES TO mm_auditor;

GRANT USAGE ON SCHEMA lic TO mm_lic_admin;
ALTER DEFAULT PRIVILEGES FOR ROLE mm_migrator IN SCHEMA lic GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO mm_lic_admin;
ALTER DEFAULT PRIVILEGES FOR ROLE mm_migrator IN SCHEMA lic GRANT USAGE, SELECT ON SEQUENCES TO mm_lic_admin;

GRANT USAGE ON SCHEMA rpt TO mm_bi_reader;
ALTER DEFAULT PRIVILEGES FOR ROLE mm_migrator IN SCHEMA rpt GRANT SELECT ON TABLES TO mm_bi_reader;

-- If tables already exist, also run the one-off equivalents, e.g.:
-- GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA mrm, sec, app, dat, rpt TO mm_app;
-- GRANT SELECT, INSERT ON ALL TABLES IN SCHEMA aud TO mm_app;
-- GRANT SELECT ON ALL TABLES IN SCHEMA lic TO mm_app;
-- GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA mrm, sec, app, dat, rpt, aud TO mm_app;
