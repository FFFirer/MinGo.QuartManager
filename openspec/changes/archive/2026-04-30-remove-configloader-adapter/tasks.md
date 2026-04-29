## 1. Simplify ConfigLoader

- [x] 1.1 Remove the obsolete `Load(string configPath)` method from ConfigLoader
- [x] 1.2 Remove the obsolete `ReplaceEnvironmentVariables()` method
- [x] 1.3 Remove the obsolete `MergeEnvironmentOverrides()` method
- [x] 1.4 Remove the `using YamlDotNet` imports if no longer needed
- [x] 1.5 Clean up any now-unused private helper methods
- [x] 1.6 Update XML documentation comments to reflect the simplified API

## 2. Update AgentExtensions

- [x] 2.1 Remove the obsolete `AddMinGoAgent(IServiceCollection, IConfiguration, string)` overload
- [x] 2.2 Keep only the `AddMinGoAgent<T>(this T builder)` extension method
- [x] 2.3 Ensure the remaining method properly integrates with IHostApplicationBuilder
- [x] 2.4 Remove `#pragma warning disable CS0618` pragmas related to the legacy path

## 3. Verify Configuration Still Works

- [x] 3.1 Build the project to ensure no compilation errors
- [x] 3.2 Check that ConfigLoader can still load and validate configuration via IConfiguration
- [x] 3.3 Verify that defaults are still applied correctly
- [x] 3.4 Ensure validation still catches missing required fields (ClusterId, Platform.Url)

## 4. Update Documentation

- [x] 4.1 Update src/MinGo.Qap.Agent/README.md to remove references to the old configuration approach
- [x] 4.2 Update any code examples showing the old AddMinGoAgent overload
- [x] 4.3 Clarify that only the builder-based approach is supported
- [x] 4.4 Document the migration path for users currently using the old overload

## 5. Final Verification

- [x] 5.1 Run dotnet build on the entire solution
- [x] 5.2 Verify no obsolete warnings remain
- [x] 5.3 Review the final changes for completeness
