## Why

ConfigLoader currently maintains two configuration loading paths:
1. New path: Uses ASP.NET Core's IConfiguration pipeline (recommended)
2. Old path: Direct YAML file parsing with manual environment variable handling (legacy)

The old path includes `ReplaceEnvironmentVariables()` and `MergeEnvironmentOverrides()` methods that duplicate functionality already provided by ASP.NET Core's configuration system. This creates technical debt, increases maintenance burden, and confuses users about which configuration approach to use.

## What Changes

- **Remove** the legacy `Load(string configPath)` method from ConfigLoader
- **Remove** `ReplaceEnvironmentVariables()` method - ASP.NET Core handles this via configuration providers
- **Remove** `MergeEnvironmentOverrides()` method - ASP.NET Core handles environment variable overrides via configuration priority chain
- **Simplify** ConfigLoader to only support the IConfiguration pipeline approach
- **Update** `AgentExtensions.cs` to remove the obsolete overload that uses the legacy path
- **Update** documentation to reflect the unified configuration approach

## Capabilities

### New Capabilities
*None - this is an internal refactoring with no new capabilities*

### Modified Capabilities
- `agent-configuration`: Configuration loading will now exclusively use ASP.NET Core IConfiguration pipeline. The old YAML-only loading path will be removed.

## Impact

- **Files Modified**: 
  - `src/MinGo.Qap.Agent/Configuration/ConfigLoader.cs`
  - `src/MinGo.Qap.Agent/AgentExtensions.cs`
  - `src/MinGo.Qap.Agent/README.md`
  - Documentation files mentioning the old configuration approach
  
- **Breaking Change**: The `AddMinGoAgent(IServiceCollection, IConfiguration, string)` overload will be removed. Users must migrate to `AddMinGoAgent<T>(this T builder)` where T : IHostApplicationBuilder.

- **Behavior**: No functional changes for users already using the new IConfiguration pipeline approach.
