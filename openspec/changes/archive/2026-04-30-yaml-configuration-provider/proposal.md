## Why

The Agent project currently loads YAML configuration (`config.yaml`) through a custom `ConfigLoader` class that bypasses the ASP.NET Core `IConfiguration` pipeline. This manual approach has several limitations: no hot-reload support, no integration with standard configuration sources (environment variables, user secrets, JSON), and incompatibility with the `IOptions<T>` pattern. A proper `IConfigurationProvider` implementation backed by YamlDotNet will integrate YAML files as a first-class citizen in the configuration pipeline, enabling unified config management, live reload, and seamless interoperability with other configuration sources.

## What Changes

- **Add `YamlConfigurationProvider`** — Custom `ConfigurationProvider` implementation that uses YamlDotNet to parse YAML files and flatten nested structures into key-value pairs compatible with `IConfiguration`
- **Add `YamlConfigurationSource`** — `IConfigurationSource` implementation that creates the provider with configurable options (file path, optional/required, reload on change)
- **Add `AddYamlFile()` extension method** — Extension on `IConfigurationBuilder` to register YAML sources in the configuration pipeline (e.g., `.AddYamlFile("config.yaml", optional: true, reloadOnChange: true)`)
- **Refactor `ConfigLoader`** — Replace manual YamlDotNet deserialization in `ConfigLoader.Load()` with the standard `IConfiguration` pipeline; `ConfigLoader` will bind from `IConfiguration` instead of doing its own YAML parsing
- **Complete `AgentExtensions.AddMinGoAgent<T>()`** — Wire up the YAML configuration source in the `IHostApplicationBuilder` overload so it works via `builder.Configuration.AddYamlFile()`
- **Add YAML configuration for Platform project** — Optionally support YAML config in the Platform project (e.g., `config.yaml` alongside `appsettings.json`)

## Capabilities

### New Capabilities
- `yaml-config-provider`: Custom `IConfigurationSource` + `IConfigurationProvider` using YamlDotNet to read YAML files into the standard ASP.NET Core configuration pipeline. Supports optional files, file watching for hot-reload, and nested YAML structure flattening.

### Modified Capabilities
- `configuration-management`: Update the existing Platform configuration-management spec to include YAML as a recognized configuration source in the priority chain. The YAML source shall sit at the same priority level as JSON (overridden by environment variables and user secrets).

## Impact

- **MinGo.Qap.Agent** — New files in `Configuration/`: `YamlConfigurationProvider.cs`, `YamlConfigurationSource.cs`, `YamlConfigurationExtensions.cs`. `ConfigLoader.cs` refactored to consume `IConfiguration` instead of doing standalone YAML parsing. `AgentExtensions.cs` updated to use the new provider.
- **MinGo.Qap.Platform** — Optionally gains YAML config support via the same shared provider (or a separate copy).
- **YamlDotNet** — Already referenced (v16.3.0), no new dependencies. May add `Microsoft.Extensions.Configuration` and `Microsoft.Extensions.FileProviders` package references if not already transitively available.
- **Tests** — New unit tests for `YamlConfigurationProvider`, `YamlConfigurationSource`, and the extension method.
- **Sample.Agent** — `config.yaml` continues to work; the sample `Program.cs` may be updated to demonstrate the new approach.
