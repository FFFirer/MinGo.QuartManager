## MODIFIED Requirements

### Requirement: Configuration must follow ASP.NET Core priority order
Platform SHALL read configuration following standard ASP.NET Core configuration provider priority, including YAML files as an additional source.

#### Scenario: Configuration priority
- **WHEN** multiple configuration sources define the same setting
- **THEN** the priority SHALL be: Environment Variables > UserSecrets > appsettings.Development.json > appsettings.json > **config.yaml**
- **AND** YAML files SHALL be treated with the same priority level as JSON files in the chain

#### Scenario: YAML file as config source
- **WHEN** an `AddYamlFile("config.yaml")` call is registered in `ConfigureAppConfiguration`
- **THEN** the settings from `config.yaml` SHALL be available through the standard `IConfiguration` API
- **AND** they SHALL be overridable by environment variables, user secrets, and ASP.NET Core JSON config files
