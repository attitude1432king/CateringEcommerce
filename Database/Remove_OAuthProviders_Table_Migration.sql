-- ============================================================
-- Migration: Remove t_sys_oauth_provider table
-- Provider config moved to appsettings.json / environment vars
-- Date: 2026-05-09
-- ============================================================

-- STEP 1: Add c_provider_key column to t_sys_user_oauth
-- Stores the provider type (e.g. 'GOOGLE') directly on each row,
-- replacing the FK relationship to t_sys_oauth_provider.
ALTER TABLE t_sys_user_oauth
    ADD COLUMN IF NOT EXISTS c_provider_key VARCHAR(50);

-- STEP 2: Backfill c_provider_key from the provider table
-- (run while t_sys_oauth_provider still exists)
UPDATE t_sys_user_oauth o
SET c_provider_key = p.c_provider_name
FROM t_sys_oauth_provider p
WHERE o.c_provider_id = p.c_provider_id;

-- STEP 3: Enforce NOT NULL after backfill
ALTER TABLE t_sys_user_oauth
    ALTER COLUMN c_provider_key SET NOT NULL;

-- STEP 4: Replace the unique constraint
ALTER TABLE t_sys_user_oauth
    DROP CONSTRAINT IF EXISTS uq_provider_user;
ALTER TABLE t_sys_user_oauth
    ADD CONSTRAINT uq_provider_key_user UNIQUE (c_provider_key, c_provider_user_id);

-- STEP 5: Replace the index on c_provider_id with one on c_provider_key
DROP INDEX IF EXISTS idx_user_oauth_provider;
CREATE INDEX IF NOT EXISTS idx_user_oauth_provider_key
    ON t_sys_user_oauth(c_provider_key);

-- STEP 6: Drop the FK constraint on c_provider_id
ALTER TABLE t_sys_user_oauth
    DROP CONSTRAINT IF EXISTS fk_user_oauth_provider;

-- STEP 7: Drop the now-orphaned c_provider_id column
ALTER TABLE t_sys_user_oauth
    DROP COLUMN IF EXISTS c_provider_id;

-- STEP 8: Drop indexes on t_sys_oauth_provider, then the table itself
DROP INDEX IF EXISTS idx_oauth_provider_name;
DROP INDEX IF EXISTS idx_oauth_active;
DROP TABLE IF EXISTS t_sys_oauth_provider;

-- ============================================================
-- VERIFICATION (run after migration to confirm success)
-- ============================================================
-- SELECT c_provider_key, COUNT(*) FROM t_sys_user_oauth GROUP BY c_provider_key;
-- Expected: GOOGLE | <n rows>

-- SELECT to_regclass('t_sys_oauth_provider');
-- Expected: NULL (table is gone)

-- \d t_sys_user_oauth
-- Expected: c_provider_key column present, c_provider_id absent
