## ADDED Requirements

### Requirement: Agent configuration SHALL support external URL override
`AgentSettings` SHALL include optional `ExternalUrl` property for explicit Agent address configuration.

#### Scenario: Explicit URL configuration
- **WHEN** `agent.externalUrl` is set in config.yaml
- **THEN** Agent SHALL use the specified URL for Platform registration
- **AND** skip all auto-detection logic

### Requirement: Agent configuration SHALL support network interface binding
`AgentSettings` SHALL include optional `NetworkInterface` property to specify which network interface to use for URL detection.

#### Scenario: Network interface selection
- **WHEN** `agent.networkInterface` is set to "eth0"
- **THEN** Agent SHALL detect IP from "eth0" interface
- **AND** construct URL using detected IP and port

### Requirement: Agent configuration SHALL support environment-specific settings
Configuration loading SHALL support environment variable overrides for all Agent settings.

#### Scenario: Environment variable override
- **WHEN** `AGENT_PORT` environment variable is set to "9090"
- **THEN** Agent SHALL use port 9090 regardless of config file value

## MODIFIED Requirements

### Requirement: Configuration must follow ASP.NET Core priority order
Agent configuration SHALL follow the same priority order as Platform: Environment Variables > UserSecrets > appsettings.Development.json > appsettings.json.

#### Scenario: Agent configuration priority
- **WHEN** multiple configuration sources define Agent settings
- **THEN** the priority SHALL be: Environment Variables > UserSecrets > appsettings.Development.json > appsettings.json

### Requirement: Connection string must support environment variable override
Production Agent deployments SHALL be able to override Quartz JobStore connection string via environment variables.

#### Scenario: Agent production environment variable
- **WHEN** `QAP_AGENT_DB_CONNECTION` environment variable is set
- **THEN** Agent SHALL use that value for Quartz ADO.NET JobStore
- **AND** log that connection string was loaded from environment (without exposing the actual string)
