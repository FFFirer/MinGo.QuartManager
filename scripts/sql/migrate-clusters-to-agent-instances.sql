-- 将现有 Clusters 迁移到 AgentInstances 表
-- 每个 Cluster 创建一个对应的 AgentInstance 记录
-- 在应用了 AddAgentInstancesAndModifyClusters 迁移后运行此脚本

INSERT INTO public."AgentInstances" (
    "Id",
    "ClusterId",
    "Name",
    "Url",
    "Status",
    "LastHeartbeat",
    "QuartzInstanceId",
    "TokenHash",
    "AgentVersion",
    "StartedAt",
    "CreatedAt",
    "UpdatedAt",
    "DeletedAt"
)
SELECT 
    gen_random_uuid()::text AS "Id",
    c."Id" AS "ClusterId",
    c."Name" || ' (Legacy)' AS "Name",
    c."AgentUrl" AS "Url",
    CASE 
        WHEN c."Status" = 0 THEN 0 -- Pending -> Pending
        WHEN c."Status" = 1 THEN 1 -- Online -> Online
        WHEN c."Status" = 2 THEN 4 -- Offline -> Offline
        WHEN c."Status" = 3 THEN 4 -- Unknown -> Offline
        ELSE 0 -- 默认 Pending
    END AS "Status",
    c."LastHeartbeat" AS "LastHeartbeat",
    NULL AS "QuartzInstanceId",
    c."TokenHash" AS "TokenHash",
    NULL AS "AgentVersion",
    CASE 
        WHEN c."LastHeartbeat" IS NOT NULL THEN c."LastHeartbeat"
        ELSE c."CreatedAt"
    END AS "StartedAt",
    c."CreatedAt" AS "CreatedAt",
    c."UpdatedAt" AS "UpdatedAt",
    c."DeletedAt" AS "DeletedAt"
FROM public."Clusters" c
WHERE c."DeletedAt" IS NULL 
    AND c."AgentUrl" IS NOT NULL 
    AND c."AgentUrl" != ''
    AND NOT EXISTS (
        SELECT 1 FROM public."AgentInstances" ai 
        WHERE ai."ClusterId" = c."Id" AND ai."Url" = c."AgentUrl"
    );

-- 更新迁移后的集群状态为 Online（如果至少有一个在线实例）
-- 注意：集群状态现在由其实例决定，此更新可选
-- 如果希望保持集群状态不变，可注释以下部分

WITH instance_status AS (
    SELECT 
        ai."ClusterId",
        MAX(CASE WHEN ai."Status" = 1 THEN 1 ELSE 0 END) AS has_online_instance
    FROM public."AgentInstances" ai
    GROUP BY ai."ClusterId"
)
UPDATE public."Clusters" c
SET "Status" = CASE 
    WHEN is_inst.has_online_instance = 1 THEN 1 -- Online
    ELSE 2 -- Offline
END
FROM instance_status is_inst
WHERE c."Id" = is_inst."ClusterId"
    AND c."DeletedAt" IS NULL;

-- 输出迁移统计
SELECT 
    COUNT(*) AS "TotalClusters",
    COUNT(CASE WHEN "AgentUrl" IS NOT NULL AND "AgentUrl" != '' THEN 1 END) AS "ClustersWithAgentUrl",
    (SELECT COUNT(*) FROM public."AgentInstances") AS "AgentInstancesCreated"
FROM public."Clusters"
WHERE "DeletedAt" IS NULL;