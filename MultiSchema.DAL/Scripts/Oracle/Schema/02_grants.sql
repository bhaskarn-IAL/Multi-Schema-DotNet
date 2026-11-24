-- =====================================================================
-- Oracle grants. Run AFTER `dotnet ef database update` (and after every
-- migration that adds tables), because Oracle 19c has no schema-level grants.
--
-- Oracle 23ai alternative (covers future tables, run once):
--   GRANT SELECT ANY TABLE, INSERT ANY TABLE, UPDATE ANY TABLE, DELETE ANY TABLE ON SCHEMA MRM TO MM_APP_ROLE;
--   ... same for SEC, APP, DAT, RPT
--   GRANT SELECT ANY TABLE, INSERT ANY TABLE ON SCHEMA AUD TO MM_APP_ROLE;
--   GRANT SELECT ANY TABLE ON SCHEMA LIC TO MM_APP_ROLE;
--   GRANT SELECT ANY TABLE ON SCHEMA SEC TO MM_IAM_READER;
--   GRANT SELECT ANY TABLE ON SCHEMA AUD TO MM_AUDITOR;
--   GRANT SELECT ANY TABLE, INSERT ANY TABLE, UPDATE ANY TABLE, DELETE ANY TABLE ON SCHEMA LIC TO MM_LIC_ADMIN;
--   GRANT SELECT ANY TABLE ON SCHEMA RPT TO MM_BI_READER;
-- =====================================================================

DECLARE
    PROCEDURE grant_all(p_schema VARCHAR2, p_privs VARCHAR2, p_grantee VARCHAR2) IS
    BEGIN
        FOR t IN (SELECT table_name FROM all_tables WHERE owner = p_schema) LOOP
            EXECUTE IMMEDIATE 'GRANT ' || p_privs || ' ON "' || p_schema || '"."' || t.table_name || '" TO ' || p_grantee;
        END LOOP;
    END;
BEGIN
    -- application
    grant_all('MRM', 'SELECT, INSERT, UPDATE, DELETE', 'MM_APP_ROLE');
    grant_all('SEC', 'SELECT, INSERT, UPDATE, DELETE', 'MM_APP_ROLE');
    grant_all('APP', 'SELECT, INSERT, UPDATE, DELETE', 'MM_APP_ROLE');
    grant_all('DAT', 'SELECT, INSERT, UPDATE, DELETE', 'MM_APP_ROLE');
    grant_all('RPT', 'SELECT, INSERT, UPDATE, DELETE', 'MM_APP_ROLE');
    grant_all('AUD', 'SELECT, INSERT',                 'MM_APP_ROLE');  -- append-only
    grant_all('LIC', 'SELECT',                         'MM_APP_ROLE');  -- read-only

    -- other parties
    grant_all('SEC', 'SELECT', 'MM_IAM_READER');
    grant_all('AUD', 'SELECT', 'MM_AUDITOR');
    grant_all('LIC', 'SELECT, INSERT, UPDATE, DELETE', 'MM_LIC_ADMIN');
    grant_all('RPT', 'SELECT', 'MM_BI_READER');

    -- cross-schema FK: MRM."ModelRecords" -> SEC."Users" needs REFERENCES granted to the FK owner
    EXECUTE IMMEDIATE 'GRANT REFERENCES ON "SEC"."Users" TO MRM';
END;
/

-- aud append-only for everyone (grants do not bind DBAs / the owner):
-- Oracle 21c+: CREATE BLOCKCHAIN / IMMUTABLE TABLE is the engine-level equivalent of SQL Server ledger.
-- Otherwise use a trigger per aud table:
DECLARE
BEGIN
    FOR t IN (SELECT table_name FROM all_tables WHERE owner = 'AUD') LOOP
        EXECUTE IMMEDIATE
            'CREATE OR REPLACE TRIGGER "AUD"."TRG_' || SUBSTR(t.table_name, 1, 100) || '_AO" ' ||
            'BEFORE UPDATE OR DELETE ON "AUD"."' || t.table_name || '" ' ||
            'BEGIN RAISE_APPLICATION_ERROR(-20001, ''aud.' || t.table_name || ' is append-only''); END;';
    END LOOP;
END;
/
