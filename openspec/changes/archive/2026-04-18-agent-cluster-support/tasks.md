## 1. Database Schema and Migration

- [x] 1.1 Create AgentInstance entity class with all required properties
- [x] 1.2 Create EF Core migration for AgentInstances table
- [x] 1.3 Update Cluster entity: make AgentUrl nullable, add navigation property to AgentInstances
- [x] 1.4 Create migration for modifying Clusters table (deprecate AgentUrl)
- [x] 1.5 Create data migration script to convert existing Clusters to AgentInstances
- [x] 1.6 Create indexes on AgentInstances table (ClusterId, Status, LastHeartbeat)

## 2. Shared Models and DTOs

- [x] 2.1 Create AgentStatus enum (Pending, Online, Warning, Offline, Deleted)
- [x] 2.2 Create AgentInstanceDto, CreateAgentRequest, AgentSummaryDto
- [x] 2.3 Create AgentRegistrationResponse with agentId and quartzInstanceId
- [x] 2.4 Update ClusterDto to include instance count instead of AgentUrl
- [x] 2.5 Create AgentHeartbeatRequest/Response DTOs
- [x] 2.6 Update existing DTOs to reference agentId instead of clusterId where appropriate

## 3. Platform Backend Services

- [x] 3.1 Create IAgentInstanceService interface with registration, heartbeat, and management methods
- [x] 3.2 Implement AgentInstanceService with token validation and instance lifecycle
- [x] 3.3 Update ClusterService to calculate status based on agent instances
- [x] 3.4 Update AgentProxyService to support instance selection strategies
- [x] 3.5 Create AgentSelectionStrategy interface with RandomStrategy implementation
- [x] 3.6 Update ClusterStatusMonitorService to monitor instance-level health
- [x] 3.7 Add instance status calculation logic (30s Warning, 60s Offline thresholds)

## 4. Platform API Endpoints

- [x] 4.1 Create AgentInstancesController with registration endpoint (POST /api/clusters/{id}/agents)
- [x] 4.2 Add instance heartbeat endpoint (POST /api/agents/{agentId}/heartbeat)
- [x] 4.3 Add instance management endpoints (GET/DELETE /api/agents/{agentId})
- [x] 4.4 Update ClustersController to handle instance-based cluster status
- [x] 4.5 Deprecate old cluster heartbeat endpoint (mark as obsolete)
- [x] 4.6 Update existing job-related endpoints to use agent instance routing
- [x] 4.7 Add cluster instances list endpoint (GET /api/clusters/{id}/agents)

## 5. Agent Modifications

- [x] 5.1 Update AgentConfig to support cluster configuration (agent.id, clusterId, platform.url)
- [x] 5.2 Create AgentRegistrationService for platform registration
- [x] 5.3 Update HeartbeatService to use instance-specific endpoint
- [x] 5.4 Modify SchedulerInitializer to support Quartz cluster configuration
- [x] 5.5 Add QuartzInstanceId generation/management logic
- [x] 5.6 Update Agent startup sequence to register before starting heartbeat
- [x] 5.7 Add graceful shutdown with deregistration

## 6. Quartz Cluster Support

- [x] 6.1 Create Quartz cluster configuration template for PostgreSQL
- [x] 6.2 Add support for both clustered and non-clustered modes in SchedulerInitializer
- [x] 6.3 Implement Quartz instance ID generation (clusterId-hostname-timestamp)
- [x] 6.4 Add Quartz cluster health monitoring to heartbeat
- [x] 6.5 Create database scripts for Quartz cluster tables (QRTZ_*)
- [x] 6.6 Document Quartz cluster setup and configuration

## 7. Frontend Updates

- [x] 7.1 Update clusterApi to support new agent instance endpoints
- [x] 7.2 Create agentInstanceApi for instance management
- [x] 7.3 Update ClustersPage to show instance count and health summary
- [x] 7.4 Create AgentInstancesPage for detailed instance management
- [x] 7.5 Add instance status indicators and health metrics display
- [x] 7.6 Update types definitions for new DTOs and responses
- [x] 7.7 Add instance registration and management UI components

## 8. Configuration and Deployment

- [x] 8.1 Update agent config.yaml template with cluster configuration options
- [x] 8.2 Create migration guide from single-instance to multi-instance
- [x] 8.3 Update docker-compose files for multi-agent deployment
- [x] 8.4 Create database migration scripts for production deployment
- [x] 8.5 Update deployment documentation with cluster considerations
- [x] 8.6 Add environment-specific configuration examples

## 9. Testing

- [ ] 9.1 Write unit tests for AgentInstanceService
- [ ] 9.2 Write unit tests for updated ClusterService status calculation
- [ ] 9.3 Write integration tests for agent registration and heartbeat
- [ ] 9.4 Test agent proxy routing with multiple instances
- [ ] 9.5 Test Quartz cluster configuration and job execution
- [ ] 9.6 Test backward compatibility with old API endpoints
- [ ] 9.7 Create end-to-end test scenario for multi-instance cluster

## 10. Documentation and Cleanup

- [x] 10.1 Update API documentation with new agent instance endpoints
- [x] 10.2 Create user guide for managing agent clusters
- [x] 10.3 Document migration process and timeline
- [ ] 10.4 Remove deprecated AgentUrl field from database (optional, after migration)
- [ ] 10.5 Remove old cluster heartbeat endpoint (after grace period)
- [x] 10.6 Update all code comments and README references