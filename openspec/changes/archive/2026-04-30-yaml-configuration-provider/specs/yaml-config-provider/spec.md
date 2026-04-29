## ADDED Requirements

### Requirement: YAML files must be loadable as a configuration source
The system SHALL provide an `AddYamlFile()` extension method on `IConfigurationBuilder` that registers YAML files as a standard configuration source in the ASP.NET Core pipeline.

#### Scenario: Register YAML file with default settings
- **WHEN** a consumer calls `builder.AddYamlFile("config.yaml")`
- **THEN** the configuration pipeline SHALL include settings from `config.yaml`
- **AND** settings from YAML SHALL be accessible via the standard `IConfiguration` API (`GetSection`, `GetValue`, `Bind`)

#### Scenario: Load YAML with optional flag
- **WHEN** a consumer calls `builder.AddYamlFile("missing.yaml", optional: true)`
- **AND** the file does not exist
- **THEN** the configuration builder SHALL NOT throw an error
- **AND** it SHALL continue with other configuration sources

#### Scenario: Load YAML with optional flag and missing file
- **WHEN** a consumer calls `builder.AddYamlFile("missing.yaml", optional: false)`
- **AND** the file does not exist
- **THEN** the configuration builder SHALL throw a `FileNotFoundException`

### Requirement: YAML files must support hot-reload
The system SHALL support reloading configuration when the YAML file changes on disk.

#### Scenario: YAML file reload on change
- **WHEN** a consumer calls `builder.AddYamlFile("config.yaml", reloadOnChange: true)`
- **AND** the YAML file is modified after the application has started
- **THEN** the configuration SHALL be reloaded
- **AND** `IOptionsSnapshot<T>` consumers SHALL receive the updated values

### Requirement: Nested YAML structures must flatten to hierarchical config keys
The provider SHALL flatten nested YAML mappings and sequences into the standard colon-delimited key format used by `IConfiguration`.

#### Scenario: Nested mapping
- **WHEN** the YAML file contains:
  ```yaml
  agent:
    clusterId: "my-cluster"
    port: 8080
  ```
- **THEN** the configuration SHALL contain keys `agent:clusterId` and `agent:port` with their respective values

#### Scenario: YAML sequence (array)
- **WHEN** the YAML file contains:
  ```yaml
  quartz:
    jobTypes:
      - "MyApp.Jobs.JobA"
      - "MyApp.Jobs.JobB"
  ```
- **THEN** the configuration SHALL contain keys `quartz:jobTypes:0` and `quartz:jobTypes:1`

#### Scenario: Mixed nested structures
- **WHEN** the YAML file contains nested mappings and sequences
- **THEN** all leaf values SHALL be reachable by their full colon-delimited path

### Requirement: Null values in YAML must be preserved
The provider SHALL handle YAML null values (`null`, `~`, empty) without throwing.

#### Scenario: Null scalar value
- **WHEN** the YAML file contains `key:` (empty) or `key: null` or `key: ~`
- **THEN** the corresponding configuration key SHALL have a `null` value

### Requirement: YAML keys must be case-insensitive
Configuration keys from YAML SHALL follow the standard `IConfiguration` case-insensitive convention.

#### Scenario: Case-insensitive access
- **WHEN** a YAML file defines `ClusterId: "value"`
- **THEN** consumers can access the value via `config["clusterid"]`, `config["CLUSTERID"]`, or `config["ClusterId"]`

### Requirement: Agent YAML config must integrate with existing ConfigLoader
The `ConfigLoader` SHALL be able to consume configuration from the `IConfiguration` pipeline instead of doing standalone YAML parsing.

#### Scenario: ConfigLoader reads from IConfiguration
- **WHEN** `ConfigLoader` is constructed with the new `IConfiguration`-based constructor
- **AND** the configuration pipeline has been populated via `AddYamlFile()`
- **THEN** `ConfigLoader.Load()` SHALL produce a correctly populated `AgentConfig` with values from the YAML source

#### Scenario: Backward-compatible constructor
- **WHEN** existing consumers call `new ConfigLoader(envConfig).Load("path")`
- **THEN** the old API SHALL continue to work without code changes
- **AND** the old constructor SHALL be marked with `[Obsolete]` with a migration message
