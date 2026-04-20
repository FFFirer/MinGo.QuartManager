## Why

The current Quartz Manager architecture supports only a single Agent instance per Cluster, which limits scalability, high availability, and fault tolerance. Many production scenarios require multiple Agent instances within the same Cluster to provide:
- **High availability** through automatic failover when instances go offline
- **Load distribution** across multiple agents for better resource utilization
- **Horizontal scaling** to handle increased job execution workloads
- **Zero-downtime deployments** by rolling updates across instances

Quartz.NET natively supports clustering, but our current implementation doesn't leverage this capability. This change will enable true agent clustering while maintaining backward compatibility with existing single-instance deployments.

## What Changes

1. **Database model extension**:
   - Add `AgentInstances` table to track multiple agent instances per cluster
   - Remove `AgentUrl` field from `Clusters` table (deprecate, then remove)
   - Add entity relationships: Cluster → multiple AgentInstances

2. **Backend service modifications**:
   - New `AgentInstanceService` for managing agent registration, heartbeats, and status
   - Modify `ClusterService` to calculate cluster status based on instance health
   - Update `AgentProxyService` to intelligently route requests to healthy instances
   - Extend `ClusterStatusMonitorService` to monitor instance-level health

3. **API changes**:
   - Add agent instance management endpoints: registration, heartbeat, listing, deletion
   - Modify cluster creation to not require `agentUrl` (optional during migration)
   - Deprecate cluster-level heartbeat endpoint in favor of instance-level
   - Add instance selection strategies for load balancing

4. **Agent enhancements**:
   - Add instance registration workflow on startup
   - Update heartbeat to use instance-specific endpoints
   - Support Quartz.NET clustered configuration (shared database job store)
   - Generate unique instance IDs for Quartz cluster participation

5. **Frontend updates**:
   - Show instance count and health in cluster overview
   - Add agent instance management interface
   - Display detailed instance metrics and status
   - Update API clients for new endpoints

**BREAKING**: Cluster-level heartbeat endpoint (`POST /api/clusters/{id}/heartbeat`) will be deprecated. Agents must use the new instance-level endpoint.

## Capabilities

### New Capabilities
- **agent-instance-management**: Register, monitor, and manage multiple agent instances within a cluster
- **agent-cluster-orchestration**: Intelligent request routing and load balancing across healthy instances
- **quartz-cluster-configuration**: Configure Quartz.NET for clustered operation with shared database persistence
- **agent-cluster-monitoring**: Monitor health, status, and metrics across all instances in a cluster
- **agent-instance-registration**: Dynamic registration of agent instances with platform and token management

### Modified Capabilities
- **database-persistence**: Extend data model to support multiple agent instances per cluster
- **configuration-management**: Update agent configuration for cluster participation and instance identification

## Impact

**Affected code**:
- `MinGo.Qap.Platform`: Cluster/AgentInstance entities, services, controllers
- `MinGo.Qap.Agent`: Configuration, registration, heartbeat, Quartz initialization
- `MinGo.Qap.Shared`: DTOs, enums, models for agent instances
- `MinGo.Qap.UI`: Cluster overview, instance management interfaces

**Database**:
- New `AgentInstances` table with foreign key to `Clusters`
- Quartz cluster tables (via Quartz.NET SQL scripts)
- Migration required for existing clusters to create initial agent instances

**APIs**:
- New endpoints for agent instance management
- Modified cluster creation and status endpoints
- Deprecated cluster-level heartbeat endpoint (with backward compatibility)

**Deployment**:
- Requires database migration before deployment
- Agent configuration updates for cluster participation
- Optional Quartz database setup for clustered operation
- Rollout strategy: platform first, then agent updates