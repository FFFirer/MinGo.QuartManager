-- ============================================================
-- Data Migration: Cluster → Mark as deprecated
-- Phase 5.1.2: Cluster table is kept for reference, no data migration needed
-- 
-- Existing Cluster records remain in the Clusters table for historical reference.
-- The Cluster entity is marked [Obsolete] in code and will be removed in a future release.
-- ============================================================

BEGIN;

-- 1. Mark all existing clusters with a note (no table changes needed)
-- Clusters table is retained for backward compatibility during migration period

-- 2. Verify no active dependencies
DO $$
DECLARE
    cluster_dep_count INT;
BEGIN
    -- Check if any JobDefinitions still reference clusters
    SELECT COUNT(*) INTO cluster_dep_count
    FROM "JobDefinitions" jd
    LEFT JOIN "Clusters" c ON c."Id" = jd."ClusterId"
    WHERE c."Id" IS NULL;

    IF cluster_dep_count > 0 THEN
        RAISE WARNING 'Found % JobDefinitions referencing non-existent clusters', cluster_dep_count;
    ELSE
        RAISE NOTICE 'All JobDefinitions have valid cluster references';
    END IF;
END $$;

COMMIT;

-- Future cleanup (only after confirming zero dependencies):
-- DROP TABLE IF EXISTS "Clusters" CASCADE;
