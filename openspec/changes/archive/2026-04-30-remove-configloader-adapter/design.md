## Context

Currently ConfigLoader maintains dual loading paths:

**New Path (ASP.NET Core IConfiguration Pipeline):**
- Uses `AddYamlFile()` to register YAML files in the configuration builder
- Leverages ASP.NET Core's built-in environment variable support
- Supports configuration reload on file change
- Proper configuration priority chain (Environment > UserSecrets > appsettings.Development.json > appsettings.json > YAML)

**Old Path (Legacy Direct YAML Parsing):**
- `Load(string configPath)` method directly reads and parses YAML files
- Manual environment variable placeholder replacement: `${VAR_NAME}` or `${VAR_NAME:default}`
- Manual environment variable override merging via `QAP_*` prefixed variables
- Duplicates functionality already provided by ASP.NET Core

The old path creates confusion about which configuration approach to use and increases maintenance burden.

## Goals / Non-Goals

**Goals:**
- Remove the legacy `Load(string configPath)` method and related obsolete methods
- Simplify ConfigLoader to only support the ASP.NET Core IConfiguration pipeline
- Ensure all configuration loading goes through the standard ASP.NET Core mechanism
- Update documentation to reflect the unified approach
- Provide clear migration path for users currently using the old overload

**Non-Goals:**
- Changing the configuration schema or available options
- Modifying how YAML files are parsed (still uses YamlDotNet)
- Changing configuration priority order
- Adding new configuration sources

## Decisions

### Decision: Remove the `AddMinGoAgent(IServiceCollection, IConfiguration, string)` overload

**Rationale:** This overload forces the legacy path by calling `configLoader.Load(configPath)`. Removing it ensures all users use the IHostApplicationBuilder extension method which properly integrates with the configuration pipeline.

**Alternative considered:** Keep the overload but make it use the new path internally. Rejected because it would require restructuring how the configuration is passed, and the builder-based approach is the idiomatic ASP.NET Core pattern.

### Decision: Remove `ReplaceEnvironmentVariables()` and `MergeEnvironmentOverrides()` methods

**Rationale:** ASP.NET Core's configuration system already handles environment variable substitution and priority. These methods are marked obsolete and are technical debt.

**Migration for users:** Environment variables can still be used via:
- `QAP_AGENT_ID` → `agent:id` mapping in appsettings.json or environment variables
- Direct environment variable configuration via `AddEnvironmentVariables()` in host builder
- User secrets for development

### Decision: Keep ConfigLoader class but simplify it

**Rationale:** Even though we're removing the legacy path, `ConfigLoader` still provides value by:
- Binding IConfiguration sections to the strongly-typed `AgentConfig`
- Validating the configuration
- Applying defaults

**Alternative considered:** Remove ConfigLoader entirely and bind directly in AgentExtensions. Rejected because the validation and default application logic is valuable to keep centralized.

## Risks / Trade-offs

**[Risk]** Users currently using `AddMinGoAgent(services, configuration, "custom.yaml")` will have broken code.
→ **Mitigation:** This is a breaking change documented in the proposal. Users need to migrate to the builder-based approach:
```csharp
// Before (will break):
services.AddMinGoAgent(configuration, "custom.yaml");

// After (correct approach):
builder.Configuration.AddYamlFile("custom.yaml", optional: true);
builder.AddMinGoAgent();
```

**[Risk]** Environment variable placeholders like `${VAR_NAME}` in YAML files won't work.
→ **Mitigation:** ASP.NET Core doesn't support this syntax natively. Users should use standard configuration hierarchy or set environment variables directly. The old syntax was a non-standard addition.

**[Risk]** Some users may rely on the specific `QAP_*` environment variable names.
→ **Mitigation:** These mappings are preserved via configuration key mapping. Users can add to appsettings.json:
```json
{
  "agent": {
    "id": "${QAP_AGENT_ID}",
    "clusterId": "${QAP_CLUSTER_ID}"
  }
}
```
Or use standard ASP.NET Core environment variable configuration with double-underscore separator: `QAP__AGENT__ID`.

## Migration Plan

1. **Phase 1 (This Change):** Remove legacy code paths
2. **Phase 2 (Documentation):** Update README and migration guide
3. **Phase 3 (Release Notes):** Mark as breaking change in release notes with migration examples

### Rollback Strategy

If critical issues are discovered:
1. Revert the commit
2. Re-release previous version
3. Address issues and retry

## Open Questions

None at this time.
