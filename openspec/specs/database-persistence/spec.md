## Requirements

### Requirement: Database connection must be configurable via environment variables
Platform SHALL read database connection string from environment variable `QAP_DB_CONNECTION` or standard ASP.NET Core connection string configuration key `ConnectionStrings:PlatformDb`.

#### Scenario: Environment variable configuration
- **WHEN** environment variable `QAP_DB_CONNECTION` is set to "Host=prod-db;Database=MinGoQap;Username=app;Password=secret"
- **THEN** Platform SHALL use this connection string for database operations

#### Scenario: UserSecrets configuration
- **WHEN** UserSecrets contains key `ConnectionStrings:PlatformDb` with a valid connection string
- **AND** no environment variable is set
- **THEN** Platform SHALL use the UserSecrets connection string

### Requirement: Database provider must be PostgreSQL
Platform SHALL use Npgsql.EntityFrameworkCore.PostgreSQL as the database provider for Entity Framework Core.

#### Scenario: PostgreSQL provider initialization
- **WHEN** PlatformDbContext is instantiated
- **THEN** it SHALL be configured with UseNpgsql() to use PostgreSQL provider

### Requirement: Database entities must persist cluster and job information
Platform SHALL store Cluster and JobDefinition entities with proper relationships and constraints.

#### Scenario: Cluster entity persistence
- **WHEN** a Cluster entity with valid data is saved
- **THEN** it SHALL be stored in the `Clusters` table with columns: Id, Name, Env, AgentUrl, Status, TokenHash, Description, CreatedAt, UpdatedAt, DeletedAt

#### Scenario: JobDefinition entity persistence
- **WHEN** a JobDefinition entity is saved with a valid ClusterId
- **THEN** it SHALL be stored in the `JobDefinitions` table with foreign key to Clusters table
- **AND** it SHALL enforce unique constraint on (ClusterId, JobKey)

### Requirement: Database must support soft delete
Clusters table SHALL implement soft delete via DeletedAt column with query filter.

#### Scenario: Soft delete filtering
- **WHEN** querying Clusters via DbContext
- **AND** a Cluster has DeletedAt set to a non-null value
- **THEN** that Cluster SHALL NOT appear in query results unless explicitly requested
