## ADDED Requirements

### Requirement: Configuration must follow ASP.NET Core priority order
Platform SHALL read connection string configuration following standard ASP.NET Core configuration provider priority.

#### Scenario: Configuration priority
- **WHEN** multiple configuration sources define connection string
- **THEN** the priority SHALL be: Environment Variables > UserSecrets > appsettings.Development.json > appsettings.json

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
