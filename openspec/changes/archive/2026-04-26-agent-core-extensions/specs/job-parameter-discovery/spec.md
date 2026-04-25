## ADDED Requirements

### Requirement: Job parameters SHALL be discoverable via custom Attribute
Agent SHALL recognize `[JobParameter]` attribute on Job class properties and constructor parameters to extract parameter metadata.

#### Scenario: Property marked with JobParameter
- **WHEN** a Job class has a property `[JobParameter("apiKey", Required = true)]`
- **THEN** Agent SHALL discover it as a required string parameter named "apiKey"

#### Scenario: Constructor parameter marked with JobParameter
- **WHEN** a Job constructor has a parameter `[JobParameter("timeout", DefaultValue = 30)]`
- **THEN** Agent SHALL discover it as an optional parameter with default value 30

### Requirement: JobParameter Attribute SHALL support metadata
`[JobParameter]` SHALL allow specifying name, description, required flag, default value, and validation regex.

#### Scenario: Full metadata specification
- **WHEN** a parameter is marked with `[JobParameter("email", Description = "User email", Required = true, ValidationRegex = @"^[^@]+@[^@]+$")]`
- **THEN** the discovered `ParameterInfoDto` SHALL contain all specified metadata

### Requirement: Job classes SHALL be discoverable with QuartzJob Attribute
Agent SHALL use `[QuartzJob]` attribute to identify Job classes and extract group and description metadata.

#### Scenario: QuartzJob attribute present
- **WHEN** a class implements `IJob` and has `[QuartzJob(Group = "sync", Description = "Data sync")]`
- **THEN** Agent SHALL register it under group "sync" with description "Data sync"

### Requirement: Complex parameters SHALL be supported via JobPayload Attribute
Agent SHALL support complex object parameters marked with `[JobPayload]` by serializing them to JSON Schema.

#### Scenario: Complex payload parameter
- **WHEN** a Job has a property of custom class type marked `[JobPayload]`
- **THEN** Agent SHALL discover it as an object-type parameter
- **AND** include its public properties in the parameter schema

### Requirement: Discovered parameters SHALL be included in Manifest
The `JobManifestDto` returned by Agent SHALL include complete parameter definitions for each job type.

#### Scenario: Manifest query returns parameters
- **WHEN** Platform queries `/api/agent/manifest`
- **THEN** each job type SHALL include its `Parameters` list with discovered metadata
