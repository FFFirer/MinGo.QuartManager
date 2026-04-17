## Why

Currently, platform logging configuration may be hardcoded or inconsistently managed. Using Serilog.AspNetCore with configuration file support provides centralized, consistent logging configuration that follows ASP.NET Core patterns.

## What Changes

- Add Serilog.AspNetCore NuGet package for structured logging
- Configure Serilog from `appsettings.json` using the `Serilog` configuration section
- Support console, file, and other sinks via configuration
- Enable minimum level, enrichers, and sink-specific settings from config
- Align with existing configuration management patterns

## Capabilities

### New Capabilities

- `serilog-config`: Configure Serilog logging via `appsettings.json` using standard Serilog configuration providers

### Modified Capabilities

- `configuration-management`: Extend to include logging configuration in appsettings.json (non-sensitive, aligns with minimal appsettings requirement)

## Impact

- New dependency: `Serilog.AspNetCore` NuGet package
- Modified: `Program.cs` / `Program.Main()` - Serilog setup
- Modified: `appsettings.json` - Add `Serilog` configuration section
- Configuration-driven logging: levels, sinks, enrichers can be changed without code changes
