## MODIFIED Requirements

### Requirement: Configuration loading SHALL use ASP.NET Core IConfiguration pipeline exclusively
Agent configuration SHALL be loaded exclusively through the ASP.NET Core IConfiguration pipeline, removing the legacy direct YAML parsing path.

#### Scenario: Using AddMinGoAgent builder extension
- **WHEN** developer calls `builder.AddMinGoAgent()` on IHostApplicationBuilder
- **THEN** ConfigLoader SHALL bind configuration from the IConfiguration pipeline
- **AND** ConfigLoader SHALL NOT directly parse YAML files
- **AND** ConfigLoader SHALL NOT manually replace environment variable placeholders

#### Scenario: Configuration priority
- **WHEN** multiple configuration sources define the same setting
- **THEN** the priority SHALL be: Environment Variables > UserSecrets > appsettings.Development.json > appsettings.json > config.yaml (via AddYamlFile)
- **AND** this SHALL be handled by ASP.NET Core's configuration system, not ConfigLoader

## REMOVED Requirements

### Requirement: Legacy direct YAML loading with environment variable placeholders
**Reason**: Removed in favor of ASP.NET Core IConfiguration pipeline which provides equivalent functionality through standard mechanisms.
**Migration**: Use `builder.Configuration.AddYamlFile("config.yaml", optional: true)` before calling `builder.AddMinGoAgent()`.

### Requirement: Manual QAP_* environment variable override merging
**Reason**: ASP.NET Core configuration system handles environment variable overrides automatically via configuration provider priority.
**Migration**: Use standard ASP.NET Core environment variables with section separator (double underscore): `QAP__AGENT__ID`, `QAP__CLUSTER__ID`, etc.

## ADDED Requirements

### Requirement: ConfigLoader SHALL support only IConfiguration-based loading
ConfigLoader SHALL only support loading configuration through the IConfiguration interface, removing all legacy file-based loading methods.

#### Scenario: ConfigLoader constructor
- **WHEN** ConfigLoader is instantiated with IConfiguration
- **THEN** it SHALL use that configuration instance exclusively
- **AND** it SHALL NOT support loading from file paths directly

#### Scenario: Simplified Load method
- **WHEN** `Load()` is called without parameters
- **THEN** it SHALL bind from IConfiguration sections (agent, platform, quartz, logging)
- **AND** it SHALL validate the configuration
- **AND** it SHALL apply defaults
- **AND** it SHALL NOT accept a file path parameter
