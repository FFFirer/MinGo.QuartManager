## ADDED Requirements

### Requirement: Platform registers agent instances
The platform SHALL provide an endpoint for agent instances to register themselves within a cluster.

#### Scenario: Successful agent registration
- **WHEN** an agent sends a registration request with valid cluster ID and token
- **THEN** the platform creates an AgentInstance record with status Pending
- **AND** returns a unique agent instance ID and optional Quartz instance ID
- **AND** stores the agent URL and metadata

#### Scenario: Registration with invalid token
- **WHEN** an agent sends a registration request with invalid or expired token
- **THEN** the platform returns HTTP 401 Unauthorized
- **AND** does not create an AgentInstance record

#### Scenario: Duplicate agent URL registration
- **WHEN** an agent attempts to register with a URL already registered to the same cluster
- **THEN** the platform returns HTTP 409 Conflict
- **AND** provides existing agent instance details

### Requirement: Platform tracks agent instance heartbeats
The platform SHALL accept and process heartbeats from registered agent instances.

#### Scenario: Successful heartbeat update
- **WHEN** an agent sends a heartbeat for a registered instance
- **THEN** the platform updates the instance's LastHeartbeat timestamp
- **AND** recalculates the instance status based on heartbeat timing
- **AND** returns HTTP 200 OK

#### Scenario: Heartbeat for unknown instance
- **WHEN** an agent sends a heartbeat for an unregistered instance ID
- **THEN** the platform returns HTTP 404 Not Found
- **AND** logs the event for monitoring

### Requirement: Platform manages agent instance lifecycle
The platform SHALL provide operations to list, retrieve, and delete agent instances.

#### Scenario: List agent instances for a cluster
- **WHEN** a user requests agent instances for a cluster
- **THEN** the platform returns a list of all agent instances with their status and metadata
- **AND** includes pagination support for large clusters

#### Scenario: Retrieve agent instance details
- **WHEN** a user requests details for a specific agent instance
- **THEN** the platform returns comprehensive instance information including:
  - Basic metadata (ID, URL, name, status)
  - Health metrics (last heartbeat, uptime, version)
  - Quartz configuration (instance ID, cluster mode)
  - Performance statistics

#### Scenario: Delete agent instance
- **WHEN** a user requests deletion of an agent instance
- **THEN** the platform marks the instance as deleted (soft delete)
- **AND** stops accepting heartbeats from that instance
- **AND** excludes the instance from cluster status calculations

### Requirement: Platform provides agent instance authentication
The platform SHALL authenticate agent instances using tokens for registration and heartbeats.

#### Scenario: Token validation for registration
- **WHEN** an agent presents a token during registration
- **THEN** the platform validates the token against the cluster's token hash
- **AND** proceeds only if the token is valid and not expired

#### Scenario: Token rotation
- **WHEN** an administrator rotates a cluster's token
- **THEN** existing agent instances continue to work with their current tokens
- **AND** new registrations require the new token
- **AND** agents can optionally re-register with the new token

### Requirement: Platform enforces agent instance limits
The platform SHALL enforce configurable limits on agent instances per cluster.

#### Scenario: Maximum instances limit reached
- **WHEN** an agent attempts to register and the cluster has reached its maximum instance limit
- **THEN** the platform returns HTTP 429 Too Many Requests
- **AND** provides information about the current limit