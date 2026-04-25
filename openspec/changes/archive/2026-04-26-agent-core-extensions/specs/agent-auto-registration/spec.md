## ADDED Requirements

### Requirement: Agent SHALL auto-detect its accessible URL
Agent SHALL implement `AgentUrlResolver` that automatically detects the Agent's externally accessible URL based on deployment environment.

#### Scenario: Kubernetes environment
- **WHEN** Agent runs in a Kubernetes pod with `POD_IP` environment variable set
- **THEN** Agent SHALL use the pod IP as its hostname
- **AND** include the configured port in the URL

#### Scenario: Docker environment
- **WHEN** Agent runs inside a Docker container
- **THEN** Agent SHALL detect container hostname or IP
- **AND** construct URL using detected address and configured port

#### Scenario: Explicit external URL configured
- **WHEN** `AgentSettings.ExternalUrl` is explicitly set in configuration
- **THEN** Agent SHALL use the configured URL without auto-detection

#### Scenario: Environment variable override
- **WHEN** `AGENT_URL` environment variable is set
- **THEN** Agent SHALL use the environment variable value
- **AND** skip all other detection logic

### Requirement: Agent SHALL register with Platform on startup
Agent SHALL automatically register itself with Platform using the resolved URL and cluster configuration.

#### Scenario: Successful registration
- **WHEN** Agent starts with valid Platform URL and API token
- **THEN** Agent SHALL POST registration request to Platform
- **AND** receive `AgentRegistrationResponse` with assigned Agent ID

#### Scenario: Registration retry
- **WHEN** initial registration fails with network error
- **THEN** Agent SHALL retry up to configured maximum attempts
- **AND** use configured retry delay between attempts

### Requirement: Agent SHALL send periodic heartbeats
Agent SHALL periodically report health status to Platform to maintain online status.

#### Scenario: Heartbeat interval
- **WHEN** Agent is registered with heartbeat interval of 30 seconds
- **THEN** Agent SHALL send heartbeat POST every 30 seconds

#### Scenario: Heartbeat response
- **WHEN** Platform responds to heartbeat with updated thresholds
- **THEN** Agent SHALL update its local warning and offline thresholds

### Requirement: Agent SHALL deregister on graceful shutdown
Agent SHALL notify Platform when shutting down to remove itself from active instances.

#### Scenario: Graceful shutdown
- **WHEN** Agent process receives shutdown signal
- **THEN** Agent SHALL send deregister request to Platform
- **AND** flush any pending logs before exit
