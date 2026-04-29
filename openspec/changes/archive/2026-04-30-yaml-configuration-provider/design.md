## Context

The `MinGo.Qap.Agent` library currently loads `config.yaml` via `ConfigLoader.Load()`, which manually reads the file, replaces environment variable placeholders, and deserializes with YamlDotNet — all outside the ASP.NET Core `IConfiguration` pipeline. The loaded `AgentConfig` POCO is registered as a singleton in DI, not via `IOptions<T>`.

This creates friction: YAML config cannot participate in standard `ConfigureAppConfiguration()`, cannot hot-reload, and the incomplete `AgentExtensions.AddMinGoAgent<T>(this T builder)` method on `IHostApplicationBuilder` is blocked (it ends at `builder.Configuration.` with no way to add YAML support).

YamlDotNet v16.3.0 is already referenced in `MinGo.Qap.Agent.csproj`. The project targets .NET 10.0 with nullable reference types enabled.

## Goals / Non-Goals

**Goals:**
- Provide `AddYamlFile()` extension method on `IConfigurationBuilder` that registers YAML files as a standard configuration source
- Support optional files, file watching for hot-reload, and nested YAML flattening (mappings, sequences, scalars)
- Integrate with the existing `ConfigLoader` so it can consume `IConfiguration` instead of doing standalone YAML parsing
- Unblock the `IHostApplicationBuilder` overload of `AddMinGoAgent<T>()`
- Maintain backward compatibility for existing `ConfigLoader.Load()` callers

**Non-Goals:**
- Not replacing `ConfigLoader` entirely — it still provides validation and default-value logic, just consuming `IConfiguration` instead of raw YAML
- Not implementing YAML-specific environment variable interpolation in the provider — the standard priority chain (env vars > file) replaces this need
- Not adding YAML support to the Platform project in this change (deferred; the provider lives in Agent and can be consumed by Platform later)
- Not changing the `AgentConfig` model classes

## Decisions

### Decision 1: Extend `FileConfigurationSource` (not raw `IConfigurationSource`)

**Chosen**: Derive `YamlConfigurationSource` from `FileConfigurationSource`.

**Rationale**: `FileConfigurationSource` provides built-in support for:
- `IFileProvider` integration (physical file system, embedded, or custom providers)
- `ReloadOnChange` — file watching via `IChangeToken`
- `Optional` — graceful handling of missing files
- `EnsureDefaults()` — wires up `Path`, `FileProvider`, and `ReloadOnChange` automatically

This is the standard pattern used by `JsonConfigurationSource`, `XmlConfigurationSource`, and all established YAML providers (NetEscapades.Configuration.Yaml, VYaml.Configuration). The alternative (raw `IConfigurationSource`) would require reimplementing all of the above.

### Decision 2: Use `YamlStream` AST visitor pattern (not `Deserializer`)

**Chosen**: Parse YAML using `YamlStream` + `YamlMappingNode`/`YamlSequenceNode` traversal, building a flat key-value dictionary via stack-based path construction.

**Rationale**: The `DeserializerBuilder().Build().Deserialize<object>()` approach returns nested `Dictionary<object,object>` and `List<object>` but sacrifices control over:
- How sequence indices are represented (as `items:0`, `items:1`)
- Null value semantics (`~`, `null`, `Null`)
- Error reporting with precise context
- Performance for large files (streaming vs full materialization)

All established production YAML providers use the AST visitor pattern. The algorithm:
1. Load YAML into `YamlStream`
2. Root is always `YamlMappingNode`
3. Recursive visitors for each node type:
   - `YamlMappingNode` → iterate children, push each key onto a context stack
   - `YamlSequenceNode` → iterate children with numeric index as context
   - `YamlScalarNode` → emit `currentPath: value` into the flat dictionary
4. Path separator: `ConfigurationPath.KeyDelimiter` (`:`)

### Decision 3: Placement in `MinGo.Qap.Agent.Configuration` namespace

**Chosen**: New provider files in `src/MinGo.Qap.Agent/Configuration/`.

```
src/MinGo.Qap.Agent/Configuration/
├── AgentConfig.cs              (existing)
├── ConfigLoader.cs              (existing, refactored)
├── YamlConfigurationProvider.cs  (new)
├── YamlConfigurationSource.cs    (new)
└── YamlConfigurationExtensions.cs (new)
```

**Rationale**: The configuration-related code already lives here. No shared project needed since only Agent references YamlDotNet. Platform can reference it later as a NuGet dependency.

### Decision 4: `ConfigLoader` migration via dual-mode constructor

**Chosen**: Add an `IConfiguration`-based constructor to `ConfigLoader`, keeping the old string-path constructor with `[Obsolete]` for backward compatibility.

```csharp
// New approach — consumes from pipeline
public ConfigLoader(IConfiguration configuration) { ... }
public AgentConfig Load() => BindFromConfiguration();

// Old approach — deprecated, internally delegates
[Obsolete("Use the IConfiguration-based constructor and ConfigureAppConfiguration with AddYamlFile()")]
public ConfigLoader(IConfiguration environmentConfig) { ... }
public AgentConfig Load(string configPath) { ... }
```

The new `Load()` reads settings via `configuration.GetSection("agent")`, `configuration.GetSection("platform")`, etc., calling the same `Validate()` and `ApplyDefaults()` methods. Environment variable overrides are handled by the standard priority chain, not manually.

### Decision 5: No built-in `${VAR_NAME}` placeholder substitution

**Chosen**: Drop the `ReplaceEnvironmentVariables` regex substitution from the provider layer.

**Rationale**: With YAML as a standard configuration source in the pipeline, environment variables take priority via `.AddEnvironmentVariables()` (called by default in `CreateBuilder`). The existing manual substitution was a workaround for the provider being outside the pipeline. Users should use `QAP_CLUSTER_ID` env vars (already supported) or standard `IConfiguration` section overrides instead of inline `${VAR_NAME}` placeholders in YAML.

If placeholder substitution is still desired for specific use cases, it can be added as a preprocessing step in a derived `YamlConfigurationProvider` (as demonstrated by OpenMod's `YamlConfigurationProviderEx`), but this is out of scope.

### Decision 6: Extension method signature

**Chosen**:
```csharp
public static IConfigurationBuilder AddYamlFile(
    this IConfigurationBuilder builder,
    string path,
    bool optional = false,
    bool reloadOnChange = false)
```

Also overloads for `IFileProvider`:
```csharp
public static IConfigurationBuilder AddYamlFile(
    this IConfigurationBuilder builder,
    IFileProvider provider,
    string path,
    bool optional = false,
    bool reloadOnChange = false)
```

**Rationale**: Mirrors the `AddJsonFile()` API surface exactly for familiarity.

## Risks / Trade-offs

| Risk | Impact | Mitigation |
|------|--------|------------|
| **YAML sequence flattening uses numeric indices** | Access pattern `section:0:key` instead of descriptive keys | This is the standard `IConfiguration` convention (same as JSON arrays). Consumers can use `GetSection("items")` and `Bind()` to typed lists. |
| **Deprecating `${VAR_NAME}` substitution** | Existing configs using inline placeholders may break | No such configs exist in the repo. The sample `config.yaml` uses plain values. Env var override via `QAP_*` is the documented and preferred approach. |
| **File watcher `ReloadOnChange` file system load** | High-frequency saves trigger repeated reloads | `FileConfigurationSource` uses `IChangeToken` with debounce. Default behavior matches `AddJsonFile()`. Test with realistic save patterns. |
| **`YamlConfigurationProvider` only flattens; typed binding deferred to consumer** | Callers must use `GetSection().Bind()` or `IOptions<T>` | This is the standard pattern. `ConfigLoader.BindFromConfiguration()` provides typed binding internally. |
| **Backward compatibility of `ConfigLoader`** | External consumers calling `ConfigLoader.Load("path")` need migration path | Keep old constructor with `[Obsolete]` for one release cycle before removal. Add clear migration guide. |

## Migration Plan

1. **Create new provider files** (`YamlConfigurationProvider.cs`, `YamlConfigurationSource.cs`, `YamlConfigurationExtensions.cs`) in `Configuration/`
2. **Add extension method** `AddYamlFile()` to `IConfigurationBuilder`
3. **Update `AgentExtensions.AddMinGoAgent<T>()`** to call `builder.Configuration.AddYamlFile("config.yaml", optional: true)`
4. **Refactor `ConfigLoader`** with dual-mode support (new `IConfiguration` ctor, deprecate old)
5. **Add unit tests** for provider parsing (scalar, nested mapping, sequence, null, empty file, missing file)
6. **Update Sample.Agent** to demonstrate the new approach
7. **Remove old code path** in next major version

## Open Questions

- Should the `AddYamlFile()` be added as an `AddMinGoAgent()` internal call, or exposed publicly for consumers to use independently? **Decision: Public**, as it's a general-purpose configuration extension, not Agent-specific. Consumers building non-Agent apps may want YAML config too.
- Should Platform get its own YAML config support in this change? **Deferred** — the provider class is in Agent but can be moved to Shared or consumed as a NuGet dependency later.
