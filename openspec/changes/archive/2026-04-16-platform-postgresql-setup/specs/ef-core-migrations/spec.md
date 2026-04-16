## ADDED Requirements

### Requirement: EF Core Migrations must be version controlled
All database schema changes SHALL be captured as EF Core Migration files stored in the `Migrations` folder.

#### Scenario: Initial migration creation
- **WHEN** `dotnet ef migrations add InitialCreate` is executed
- **THEN** it SHALL generate migration files containing Clusters and JobDefinitions table definitions
- **AND** these files SHALL be committed to version control

### Requirement: Migrations must be applicable via CLI
Developers MUST be able to apply migrations using Entity Framework Core CLI tools.

#### Scenario: Apply migrations via CLI
- **WHEN** `dotnet ef database update` is executed with valid connection string
- **THEN** all pending migrations SHALL be applied to the database
- **AND** the database SHALL contain the expected schema

### Requirement: DesignTimeDbContextFactory must support CLI operations
Platform SHALL provide a DesignTimeDbContextFactory to enable `dotnet ef` commands without running the application.

#### Scenario: Design-time context creation
- **WHEN** `dotnet ef migrations add` is executed
- **THEN** DesignTimeDbContextFactory SHALL create a DbContext instance
- **AND** it SHALL read connection string from environment or UserSecrets

### Requirement: Development environment must auto-apply migrations
Platform SHALL automatically apply pending migrations on startup in Development environment.

#### Scenario: Development auto-migration
- **WHEN** application starts in Development environment
- **AND** database exists but is missing migrations
- **THEN** Platform SHALL automatically apply all pending migrations
- **AND** log the applied migration names

### Requirement: Production environment must not auto-apply migrations
Platform SHALL NOT automatically apply migrations in Production environment.

#### Scenario: Production migration safety
- **WHEN** application starts in Production environment
- **AND** database has pending migrations
- **THEN** Platform SHALL NOT automatically apply them
- **AND** it SHALL log a warning about pending migrations
