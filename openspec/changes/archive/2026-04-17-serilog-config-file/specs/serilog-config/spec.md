## ADDED Requirements

### Requirement: Serilog must be configured via appsettings.json

Platform SHALL configure Serilog using the `Serilog` section in `appsettings.json` files via `ReadFrom.Configuration()`.

#### Scenario: Configuration file structure
- **WHEN** examining `appsettings.json`
- **THEN** it SHALL contain a `Serilog` configuration section
- **AND** it SHALL include `MinimumLevel` settings

### Requirement: Production environment must have no output by default

Platform SHALL have no log output configured in the default `appsettings.json` (no WriteTo section).

#### Scenario: Default configuration is silent
- **WHEN** application runs with `appsettings.json` only
- **THEN** no log output SHALL be produced
- **AND** logs SHALL be discarded

### Requirement: Development environment must output to Console with SourceContext

Platform SHALL output structured logs to console in development environment with SourceContext visible.

#### Scenario: Console output format
- **WHEN** application runs in Development environment
- **THEN** logs SHALL be written to console
- **AND** the output template SHALL include SourceContext
- **AND** the format SHALL be: `[HH:mm:ss Level] [SourceContext] Message`

#### Scenario: Development overrides production config
- **WHEN** examining `appsettings.Development.json`
- **THEN** it SHALL contain a `Serilog` section with `WriteTo` array
- **AND** the first WriteTo SHALL be Console
- **AND** it SHALL override the default Silent configuration
