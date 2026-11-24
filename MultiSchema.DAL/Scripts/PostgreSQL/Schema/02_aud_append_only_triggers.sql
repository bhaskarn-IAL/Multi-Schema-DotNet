-- =====================================================================
-- PostgreSQL: make aud append-only even for the table owner (grants do
-- not bind owners). Run AFTER `dotnet ef database update`.
-- =====================================================================

CREATE OR REPLACE FUNCTION aud.reject_mutation() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
    RAISE EXCEPTION 'aud.% is append-only: % is not allowed', TG_TABLE_NAME, TG_OP
        USING ERRCODE = 'insufficient_privilege';
END $$;

DO $$
DECLARE t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname = 'aud' LOOP
        EXECUTE format('DROP TRIGGER IF EXISTS trg_append_only ON aud.%I', t);
        EXECUTE format('CREATE TRIGGER trg_append_only BEFORE UPDATE OR DELETE ON aud.%I
                        FOR EACH ROW EXECUTE FUNCTION aud.reject_mutation()', t);
        EXECUTE format('DROP TRIGGER IF EXISTS trg_append_only_truncate ON aud.%I', t);
        EXECUTE format('CREATE TRIGGER trg_append_only_truncate BEFORE TRUNCATE ON aud.%I
                        FOR EACH STATEMENT EXECUTE FUNCTION aud.reject_mutation()', t);
    END LOOP;
END $$;
