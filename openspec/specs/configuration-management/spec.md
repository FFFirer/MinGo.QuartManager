## Requirements

### Requirement: Configuration must follow ASP.NET Core priority order
Platform SHALL read connection string configuration following standard ASP.NET Core configuration provider priority.

#### Scenario: Connection string priority
- **WHEN** multiple configuration sources define connection string
- **THEN** the priority SHALL be: Environment Variables > UserSecrets > appsettings.Development.json > appsettings.json

#### Scenario: Agent configuration priority
- **WHEN** multiple configuration sources define Agent settings (Agent.ClusterId, Platform.Url, etc.)
- **THEN** the priority SHALL be: Environment Variables > UserSecrets > appsettings.Development.json > appsettings.json > config.yaml (via AddYamlFile)
- **AND** this SHALL be handled by ASP.NET Core's configuration system, not by custom code

### Requirement: appsettings.json must be minimal
appsettings.json SHALL contain only non-sensitive configuration (logging, Swagger, etc.) without connection strings.

#### Scenario: Minimal appsettings.json
- **WHEN** examining `appsettings.json`
- **THEN** it SHALL NOT contain `ConnectionStrings` section
- **AND** it SHALL NOT contain database credentials

### Requirement: appsettings.Development.json must include development connection string
appsettings.Development.json SHALL include a development-friendly connection string for local PostgreSQL instance.

#### Scenario: Development connection string
- **WHEN** examining `appsettings.Development.json`
- **THEN** it SHALL contain `ConnectionStrings:PlatformDb` with localhost defaults
- **AND** it SHALL use standard PostgreSQL development credentials (postgres/postgres)

### Requirement: Connection string must support environment variable override
Production deployments SHALL be able to override connection string via environment variables without modifying files.

#### Scenario: Production environment variable
- **WHEN** `QAP_DB_CONNECTION` environment variable is set before starting application
- **THEN** Platform SHALL use that value instead of any file-based configuration
- **AND** it SHALL log that connection string was loaded from environment (without exposing the actual string)

### Requirement: UserSecrets must be supported for development
Developers SHALL be able to use `dotnet user-secrets` to store connection strings locally without committing to repository.

#### Scenario: UserSecrets usage
- **WHEN** developer runs `dotnet user-secrets set "ConnectionStrings:PlatformDb" "Host=localhost;..."`
- **AND** then runs the application
- **THEN** Platform SHALL use the UserSecrets value
- **AND** .gitignore SHALL exclude secrets from version control

### Requirement: Agent configuration SHALL load via IOptions pattern with validation
Agent configuration SHALL be loaded through the standard ASP.NET Core IOptions pattern with IConfigureOptions, IPostConfigureOptions, and IValidateOptions.

#### Scenario: Agent configuration via AddMinGoAgent
- **WHEN** developer calls `builder.AddMinGoAgent()` on IHostApplicationBuilder
- **THEN** `config.yaml` SHALL be registered as an optional YAML configuration source
- **AND** AgentConfig SHALL be configured via `IConfigureOptions<AgentConfig>` binding from IConfiguration sections (agent, platform, quartz, logging)
- **AND** default values SHALL be applied via `IPostConfigureOptions<AgentConfig>`
- **AND** validation SHALL be performed via `IValidateOptions<AgentConfig>` on startup

#### Scenario: Agent configuration validation on startup
- **WHEN** AgentConfig.ClusterId is missing or empty
- **THEN** the application SHALL fail to start with a validation error
- **WHEN** AgentConfig.Platform.Url is missing or invalid
- **THEN** the application SHALL fail to start with a validation error
- **WHEN** AgentConfig.Agent.Port is outside 1-65535
- **THEN** the application SHALL fail to start with a validation error
