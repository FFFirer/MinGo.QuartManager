## MODIFIED Requirements

### Requirement: appsettings.json must be minimal

The requirement is updated to include Serilog configuration as an allowed non-sensitive configuration.

**FROM:**
appsettings.json SHALL contain only non-sensitive configuration without connection strings.

**TO:**
appsettings.json SHALL contain only non-sensitive configuration (logging, Serilog, Swagger, etc.) without connection strings.

#### Scenario: Minimal appsettings.json
- **WHEN** examining `appsettings.json`
- **THEN** it SHALL NOT contain `ConnectionStrings` section
- **AND** it SHALL NOT contain database credentials
- **AND** it SHALL contain `Serilog` section for logging configuration
