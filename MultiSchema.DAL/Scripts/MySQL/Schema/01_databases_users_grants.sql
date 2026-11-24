-- =====================================================================
-- MySQL. "SCHEMA" and "DATABASE" are the same thing in MySQL.
--
-- Database mode (default, enMySqlSchemaMode.Database):
--   each logical schema is a database on the SAME server; the one EF connection
--   reaches all of them (`mrm`.`ModelRecords` JOIN `sec`.`Users`), cross-database
--   FKs and transactions work (InnoDB), and grants are per schema like the other engines.
--   The connection string's Database= should point at `app` (holds __EFMigrationsHistory).
--
-- Prefix mode (enMySqlSchemaMode.Prefix): only if you are limited to one database.
--   Tables become mrm_ModelRecords etc. and permissions must be table-level (section B).
-- =====================================================================

-- ---------- A. Database mode -----------------------------------------
CREATE DATABASE IF NOT EXISTS mrm;
CREATE DATABASE IF NOT EXISTS sec;
CREATE DATABASE IF NOT EXISTS aud;
CREATE DATABASE IF NOT EXISTS lic;
CREATE DATABASE IF NOT EXISTS app;
CREATE DATABASE IF NOT EXISTS dat;
CREATE DATABASE IF NOT EXISTS rpt;

CREATE USER IF NOT EXISTS 'mm_migrator'@'%' IDENTIFIED BY 'change-me';
CREATE USER IF NOT EXISTS 'mm_app'@'%'      IDENTIFIED BY 'change-me';
CREATE ROLE IF NOT EXISTS 'mm_iam_reader', 'mm_auditor', 'mm_lic_admin', 'mm_bi_reader';

-- migrator owns DDL on all seven
GRANT ALL PRIVILEGES ON mrm.* TO 'mm_migrator'@'%';
GRANT ALL PRIVILEGES ON sec.* TO 'mm_migrator'@'%';
GRANT ALL PRIVILEGES ON aud.* TO 'mm_migrator'@'%';
GRANT ALL PRIVILEGES ON lic.* TO 'mm_migrator'@'%';
GRANT ALL PRIVILEGES ON app.* TO 'mm_migrator'@'%';
GRANT ALL PRIVILEGES ON dat.* TO 'mm_migrator'@'%';
GRANT ALL PRIVILEGES ON rpt.* TO 'mm_migrator'@'%';

-- application
GRANT SELECT, INSERT, UPDATE, DELETE ON mrm.* TO 'mm_app'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON sec.* TO 'mm_app'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON app.* TO 'mm_app'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON dat.* TO 'mm_app'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON rpt.* TO 'mm_app'@'%';
GRANT SELECT, INSERT                 ON aud.* TO 'mm_app'@'%';   -- append-only
GRANT SELECT                         ON lic.* TO 'mm_app'@'%';   -- read-only

-- other parties
GRANT SELECT ON sec.* TO 'mm_iam_reader';
GRANT SELECT ON aud.* TO 'mm_auditor';
GRANT SELECT, INSERT, UPDATE, DELETE ON lic.* TO 'mm_lic_admin';
GRANT SELECT ON rpt.* TO 'mm_bi_reader';

-- ---------- B. Prefix mode (single database) --------------------------
-- USE multischema;
-- GRANT SELECT, INSERT ON multischema.aud_AuditTrail      TO 'mm_app'@'%';
-- GRANT SELECT, INSERT ON multischema.aud_WorkflowHistory TO 'mm_app'@'%';
-- GRANT SELECT, INSERT ON multischema.aud_LicenceEvidence TO 'mm_app'@'%';
-- GRANT SELECT ON multischema.lic_LicenceInstallations    TO 'mm_app'@'%';
-- ... one GRANT per table; generate with:
-- SELECT CONCAT('GRANT SELECT ON multischema.`', table_name, '` TO ''mm_bi_reader'';')
--   FROM information_schema.tables WHERE table_schema = 'multischema' AND table_name LIKE 'rpt\_%';
