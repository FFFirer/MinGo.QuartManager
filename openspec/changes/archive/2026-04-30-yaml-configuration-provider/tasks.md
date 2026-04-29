## 1. Core Provider Implementation

- [x] 1.1 Create `YamlConfigurationSource.cs` — extend `FileConfigurationSource`, override `Build()` to return `YamlConfigurationProvider`
- [x] 1.2 Create `YamlConfigurationProvider.cs` — extend `FileConfigurationProvider`, implement `Load(Stream)` with `YamlStream` visitor-based flattening
- [x] 1.3 Implement `YamlConfigurationStreamParser` — recursive AST visitor for `YamlMappingNode`, `YamlSequenceNode`, `YamlScalarNode` with stack-based path construction
- [x] 1.4 Handle null/empty YAML values (`null`, `~`, empty scalar) mapping to `null` in the `Data` dictionary
- [x] 1.5 Ensure keys use `StringComparer.OrdinalIgnoreCase` in the `Data` dictionary for case-insensitive access

## 2. Extension Method

- [x] 2.1 Create `YamlConfigurationExtensions.cs` with `AddYamlFile(this IConfigurationBuilder, string path, bool optional, bool reloadOnChange)`
- [x] 2.2 Add `AddYamlFile(this IConfigurationBuilder, IFileProvider provider, string path, bool optional, bool reloadOnChange)` overload
- [x] 2.3 Ensure `YamlConfigurationSource.EnsureDefaults()` is called in `Build()` to wire up `FileProvider`, `Path`, and `ReloadOnChange`

## 3. ConfigLoader Refactoring

- [x] 3.1 Add new `IConfiguration`-based constructor to `ConfigLoader`: `public ConfigLoader(IConfiguration configuration)`
- [x] 3.2 Implement `Load()` (parameterless) that reads `AgentConfig` from `IConfiguration` sections via `GetSection("agent")`, `GetSection("platform")`, `GetSection("quartz")`, `GetSection("logging")`
- [x] 3.3 Mark old `ConfigLoader(IConfiguration)` constructor and `Load(string)` as `[Obsolete]` with migration message
- [x] 3.4 Keep `Validate()` and `ApplyDefaults()` logic shared between old and new paths
- [x] 3.5 Remove `ReplaceEnvironmentVariables()` method — no longer needed (env vars handled by standard pipeline priority)

## 4. AgentExtensions Integration

- [x] 4.1 Complete the `AddMinGoAgent<T>(this T builder)` overload on `IHostApplicationBuilder` to call `builder.Configuration.AddYamlFile("config.yaml", optional: true)`
- [x] 4.2 Verify the existing `AddMinGoAgent(this IServiceCollection, IConfiguration, string)` still works via `ConfigLoader` new path
- [x] 4.3 Add `using Microsoft.Extensions.Configuration` to `AgentExtensions.cs` if missing

## 5. Testing

- [x] 5.1 Add test `Load_YamlFile_ReturnsConfigValues` — basic YAML with nested mapping and scalar values
- [x] 5.2 Add test `Load_YamlFile_WithSequence_FlattensToIndexedKeys` — YAML array flattened to `:0`, `:1` keys
- [ ] 5.3 Add test `Load_YamlFile_Optional_MissingFile_DoesNotThrow` — `optional: true` with nonexistent file **(requires file I/O integration test)**
- [ ] 5.4 Add test `Load_YamlFile_NotOptional_MissingFile_Throws` — `optional: false` with nonexistent file **(requires file I/O integration test)**
- [x] 5.5 Add test `Load_YamlFile_NullValues_HandledGracefully` — null, `~`, and empty scalars
- [ ] 5.6 Add test `Load_YamlFile_ReloadOnChange_DetectsFileModification` — hot-reload via file change token **(requires file watcher infrastructure)**
- [ ] 5.7 Add test `ConfigLoader_BindsFromConfiguration` — new `ConfigLoader(IConfiguration)` produces correct `AgentConfig`
- [ ] 5.8 Add test `ConfigLoader_OldConstructor_BackwardCompatible` — old API still works with deprecation warning

## 6. Sample & Documentation

- [x] 6.1 Update `samples/Sample.Agent/Program.cs` to demonstrate `builder.AddMinGoAgent()` (new overload)
- [ ] 6.2 Update `src/MinGo.Qap.Agent/README.md` to document the new `AddYamlFile()` extension method
- [x] 6.3 Verify `config.yaml` still works with the sample after migration
