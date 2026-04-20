## Context

The current MinGo.QuartzManager architecture follows a 1:1 relationship between Clusters and Agents. Each `Cluster` record contains a single `AgentUrl` field, and all communication (heartbeats, job management) targets that single agent instance. This limits the system's ability to provide high availability, load distribution, and horizontal scaling.

Quartz.NET natively supports clustering via shared database persistence, allowing multiple scheduler instances to coordinate job execution. However, our platform doesn't expose this capability at the management level, nor does it provide instance-level monitoring and orchestration.

This design addresses how to extend the system to support multiple Agent instances per Cluster while maintaining backward compatibility and providing a clear migration path for existing deployments.

## Goals / Non-Goals

**Goals:**
1. Support multiple Agent instances within a single Cluster
2. Provide automatic instance health monitoring and status calculation
3. Enable intelligent request routing to healthy instances
4. Support Quartz.NET clustered configuration for job execution coordination
5. Maintain backward compatibility with existing single-instance deployments during migration
6. Provide instance-level visibility and management in the UI
7. Enable zero-downtime deployments and rolling updates

**Non-Goals:**
1. Automatic instance discovery (instances are manually registered)
2. Complex load balancing algorithms beyond random selection
3. Cross-cluster instance sharing or migration
4. Dynamic instance scaling based on metrics (manual scaling only)
5. Job execution history aggregation across instances (remains per-instance)

## Decisions

### 1. Data Model: Separate AgentInstances Table
**Decision**: Create a new `AgentInstances` table rather than extending `Clusters` with array fields.
**Rationale**:
- Clear separation of concerns: Clusters manage logical grouping, AgentInstances manage physical deployments
- Better query performance with indexed foreign key relationships
- Easier to add instance-specific metadata (version, uptime, metrics)
- Aligns with relational database best practices
- Allows soft deletion of individual instances without affecting the cluster

**Alternative Considered**: Add `AgentUrls` array field to `Clusters` table
- Simpler schema but limits instance-specific metadata
- Harder to query and index individual instance status
- Less flexible for future instance-level operations

### 2. Agent Instance Identification
**Decision**: Use two-level identification: `Id` (database PK) and `QuartzInstanceId` (Quartz scheduler instance ID).
**Rationale**:
- `Id` provides stable database reference for platform operations
- `QuartzInstanceId` can be auto-generated or configured for Quartz clustering
- Separation allows reusing `Id` even if Quartz instance changes
- Supports both clustered (`AUTO`) and non-clustered (`NON_CLUSTERED`) modes

**Alternative Considered**: Single ID serving both purposes
- Simpler but couples platform identity with Quartz implementation
- Limits flexibility in Quartz configuration

### 3. Status Calculation Hierarchy
**Decision**: Instance status drives cluster status, not the other way around.
**Rationale**:
- Clear causal relationship: instance health → cluster health
- Easier to debug and monitor root causes
- Allows for nuanced statuses (e.g., cluster with mixed healthy/warning instances)
- Aligns with microservices health pattern

**Status Mapping**:
- **Cluster Online**: At least one instance is Online
- **Cluster Warning**: No Online instances, but at least one Warning instance  
- **Cluster Offline**: All instances Offline or no instances

### 4. Request Routing Strategy
**Decision**: Simple random selection from healthy instances.
**Rationale**:
- Minimal complexity for initial implementation
- Provides basic load distribution
- Easy to extend later with round-robin, least-connections, or weighted strategies
- Failure handling: retry with another instance on request failure

**Alternative Considered**: Round-robin selection
- More even distribution but requires state tracking
- More complex for initial implementation
- Can be added later as a configurable strategy

### 5. Quartz Cluster Configuration Approach
**Decision**: Support both clustered and non-clustered modes via configuration.
**Rationale**:
- Allows gradual adoption: start with non-clustered, move to clustered
- Non-clustered useful for development and simple deployments
- Clustered required for production high availability
- Configuration-driven approach provides flexibility

**Clustered Mode Requirements**:
- Shared database (PostgreSQL) with Quartz tables
- `quartz.jobStore.clustered = true`
- Unique `quartz.scheduler.instanceId` per instance
- Network time synchronization

### 6. Backward Compatibility Strategy
**Decision**: Phased migration with temporary dual support.
**Rationale**:
1. **Phase 1**: Platform supports both old and new APIs
2. **Phase 2**: Update Agents to use new registration/heartbeat
3. **Phase 3**: Deprecate old APIs, migrate remaining Agents
4. **Phase 4**: Remove old APIs and data fields

**Migration Support**:
- Legacy `AgentUrl` field remains during migration
- Platform can auto-create AgentInstance from existing Cluster data
- Old heartbeat endpoint continues to work (maps to default instance)
- Documentation and tooling for migration

## Risks / Trade-offs

### [Risk] Database Migration Complexity
**Mitigation**: 
- Use EF Core migrations with careful rollback plan
- Test migration on staging environment first
- Create backup before production migration
- Phase migration: add table first, then migrate data, then remove old field

### [Risk] Quartz Cluster Configuration Errors
**Mitigation**:
- Provide detailed configuration examples and validation
- Include health checks for Quartz cluster connectivity
- Monitor Quartz tables for locks and deadlocks
- Support fallback to non-clustered mode

### [Risk] Request Routing Failures
**Mitigation**:
- Implement retry logic with circuit breaker pattern
- Log instance selection and failures for debugging
- Provide admin override to specify target instance
- Health checks before routing requests

### [Risk] Instance Registration Race Conditions
**Mitigation**:
- Use unique constraints on (ClusterId, Url) to prevent duplicates
- Token validation to prevent unauthorized registration
- Registration with lease expiration (require periodic re-registration)
- Admin approval workflow for production clusters

### [Trade-off] Simplicity vs. Flexibility
**Choice**: Start simple (random selection, basic monitoring) and extend later.
**Reason**: Core requirement is multi-instance support, not advanced orchestration. Advanced features (load balancing, auto-scaling) can be added incrementally after validating the core architecture.

### [Trade-off] Immediate Migration vs. Long Dual Support
**Choice**: Provide 1-2 month migration window with dual support.
**Reason**: Existing deployments need time to update. Breaking changes should be communicated well in advance with clear migration path.

## Migration Plan

### Phase 1: Platform Update (Week 1-2)
1. Deploy updated Platform with new `AgentInstances` table
2. Run database migration (adds table, keeps `AgentUrl` field)
3. Auto-create AgentInstance for each existing Cluster
4. New APIs active, old APIs continue working
5. UI updates to show instance count

### Phase 2: Agent Updates (Week 3-4)
1. Update Agent configuration template for cluster support
2. Deploy updated Agents with registration support
3. Agents switch to instance-level heartbeat
4. Monitor for issues, rollback if needed

### Phase 3: Cleanup (Week 5-6)
1. Migrate any remaining Agents using old APIs
2. Remove deprecated API endpoints after grace period
3. Optional: remove `AgentUrl` field from database
4. Update documentation for new architecture

### Rollback Strategy
- **Phase 1 rollback**: Revert Platform, database migration rollback
- **Phase 2 rollback**: Revert Agents to previous version, continue using old APIs
- **Phase 3 rollback**: Not applicable (removal of deprecated features)

## Open Questions

1. **Instance Naming Convention**: Should instance names be auto-generated (`agent-001`) or user-provided? 
   - Proposal: Auto-generate but allow optional override for display purposes

2. **Quartz Instance ID Generation**: How to ensure uniqueness across deployments?
   - Proposal: Use `{clusterId}-{machineName}-{timestamp}` pattern

3. **Heartbeat Failure Thresholds**: What are optimal thresholds for Warning/Offline?
   - Proposal: Warning at 30 seconds, Offline at 60 seconds (configurable)

4. **Instance Token Rotation**: Should tokens auto-rotate? How often?
   - Proposal: Manual rotation only initially, with audit logging

5. **UI Refresh Interval**: How often should instance status update?
   - Proposal: 30 seconds for near-real-time monitoring