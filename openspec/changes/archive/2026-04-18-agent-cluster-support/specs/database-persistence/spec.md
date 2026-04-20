## MODIFIED Requirements

### Requirement: Database entities must persist cluster and job information
Platform SHALL store Cluster, AgentInstance, and JobDefinition entities with proper relationships and constraints.

#### Scenario: Cluster entity persistence
- **WHEN** a Cluster entity with valid data is saved
- **THEN** it SHALL be stored in the `Clusters` table with columns: Id, Name, Env, Status, TokenHash, Description, CreatedAt, UpdatedAt, DeletedAt
- **AND** it SHALL NOT require AgentUrl column (deprecated, may be nullable during migration)

#### Scenario: AgentInstance entity persistence
- **WHEN** an AgentInstance entity with valid ClusterId is saved
- **THEN** it SHALL be stored in the `AgentInstances` table with columns: Id, ClusterId, Name, Url, Status, LastHeartbeat, QuartzInstanceId, TokenHash, AgentVersion, StartedAt, CreatedAt, UpdatedAt, DeletedAt
- **AND** it SHALL enforce foreign key constraint to Clusters table
- **AND** it SHALL enforce unique constraint on (ClusterId, Url) to prevent duplicate instances

#### Scenario: JobDefinition entity persistence
- **WHEN** a JobDefinition entity is saved with a valid ClusterId
- **THEN** it SHALL be stored in the `JobDefinitions` table with foreign key to Clusters table
- **AND** it SHALL enforce unique constraint on (ClusterId, JobKey)
- **AND** JobDefinitions SHALL be associated with Cluster, not with individual AgentInstances

## ADDED Requirements

### Requirement: Database must support agent instance relationships
Platform SHALL maintain proper relationships between Clusters and their AgentInstances.

#### Scenario: One-to-many relationship enforcement
- **WHEN** a Cluster has multiple AgentInstances
- **THEN** each AgentInstance SHALL reference the Cluster via foreign key
- **AND** querying a Cluster SHALL include all its AgentInstances via navigation property
- **AND** deleting a Cluster SHALL cascade delete all its AgentInstances (soft delete)

#### Scenario: AgentInstance status tracking
- **WHEN** an AgentInstance is created
- **THEN** it SHALL have initial status Pending
- **AND** SHALL have nullable LastHeartbeat column
- **AND** SHALL have CreatedAt timestamp set to current UTC time

### Requirement: Database must support Quartz cluster tables
Platform SHALL support separate Quartz database tables for clustered job execution.

#### Scenario: Quartz database schema
- **WHEN** using Quartz clustered mode
- **THEN** the system SHALL use separate database tables with QRTZ_ prefix
- **AND** these tables SHALL be separate from Platform database tables
- **AND** SHALL follow Quartz.NET standard schema definitions

#### Scenario: Quartz instance identification storage
- **WHEN** an AgentInstance is configured for Quartz clustering
- **THEN** its QuartzInstanceId SHALL be stored in the AgentInstances table
- **AND** this ID SHALL be unique across all instances in the same Quartz cluster
- **AND** SHALL be used for Quartz scheduler instance identification