-- ============================================================
-- Data Migration: AgentInstance → Agent
-- Phase 5.1.1: Migrate existing AgentInstance records to the new Agent table
-- 
-- Run this AFTER the EF Core migration that creates the new tables
-- (Agents, SchedulerInfos, AgentSchedulers)
-- ============================================================

BEGIN;

-- 1. Migrate AgentInstance data to Agent
INSERT INTO "Agents" (
    "Id",
    "Name",
    "Url",
    "Status",
    "AgentVersion",
    "TokenHash",
    "LastHeartbeat",
    "LastReportedAt",
    "StartedAt",
    "CreatedAt",
    "UpdatedAt",
    "DeletedAt"
)
SELECT
    ai."Id",
    COALESCE(ai."Name", 'agent-' || SUBSTRING(ai."Id" FROM 1 FOR 8)),
    ai."Url",
    CASE ai."Status"
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Online'
        WHEN 2 THEN 'Warning'
        WHEN 3 THEN 'Offline'
        WHEN 4 THEN 'Offline'  -- Deleted → Offline
        ELSE 'Pending'
    END,
    ai."AgentVersion",
    ai."TokenHash",
    ai."LastHeartbeat"::timestamptz,
    NULL,  -- LastReportedAt (no equivalent in old schema)
    COALESCE(ai."StartedAt"::timestamptz, ai."CreatedAt"::timestamptz),
    ai."CreatedAt"::timestamptz,
    COALESCE(ai."UpdatedAt"::timestamptz, ai."CreatedAt"::timestamptz),
    ai."DeletedAt"::timestamptz
FROM "AgentInstances" ai
WHERE NOT EXISTS (
    SELECT 1 FROM "Agents" a WHERE a."Id" = ai."Id"
);

-- 2. Update sequences (if any) - optional

-- 3. Verify migration
DO $$
DECLARE
    agent_count INT;
    instance_count INT;
BEGIN
    SELECT COUNT(*) INTO agent_count FROM "Agents";
    SELECT COUNT(*) INTO instance_count FROM "AgentInstances" WHERE "DeletedAt" IS NULL;
    RAISE NOTICE 'Migration: Agent table has % records, AgentInstances has % active records', agent_count, instance_count;
END $$;

COMMIT;

-- Note: After migration, verify data consistency by running:
-- SELECT * FROM "Agents" a
-- LEFT JOIN "AgentInstances" ai ON ai."Id" = a."Id"
-- WHERE ai."Id" IS NULL;
