## Context

Platform currently uses default ASP.NET Core logging with `ILogger<T>` and basic `appsettings.json` configuration. Serilog.AspNetCore will provide structured logging with better sink options (console with formatted output, file rolling logs) that can be configured via `appsettings.json`.

Current state:
- `Program.cs`: No Serilog setup, uses `WebApplication.CreateBuilder(args)` default logging
- `appsettings.json`: Basic `Logging` section with `LogLevel` only

## Goals / Non-Goals

**Goals:**
- Add Serilog.AspNetCore for structured logging
- Configure Serilog entirely from `appsettings.json` using `Serilog` configuration section
- Development: Console output with SourceContext
- Production: No output configured by default
- Maintain existing `ILogger<T>` usage (Serilog provides the implementation)

**Non-Goals:**
- Custom log enrichers beyond basic properties
- Structured log query/aggregation infrastructure
- Rolling file cleanup automation
- Serilog self-logging configuration

## Decisions

### 1. Use Serilog.AspNetCore over raw Serilog

**Decision:** Use `Serilog.AspNetCore` package

**Rationale:** Provides `UseSerilog()` extension method that properly integrates with ASP.NET Core's `IServiceCollection` and lifecycle (flush on shutdown). Aligns with Microsoft-recommended pattern for Serilog in ASP.NET Core applications.

### 2. Configure Serilog via `appsettings.json` WriteTo section

**Decision:** Use `ReadFrom.Configuration(builder.Configuration)` to read `Serilog` section

**Rationale:** Serilog has built-in configuration provider that maps `appsettings.json` `Serilog` section to `LoggerConfiguration`. Keeps all logging config in one place without custom code.

### 3. Remove existing Logging section

**Decision:** Remove the default ASP.NET Core `Logging` section from `appsettings.json`

**Rationale:** When using Serilog, the `appsettings.json` `Logging` section is ignored. Having both is confusing. Keep only `Serilog` section.

### 4. Environment-specific configuration

**Decision:** Default `appsettings.json` has minimal Serilog config (no WriteTo); `appsettings.Development.json` overrides with Console sink

**Rationale:** Production stays silent unless explicitly configured. Development gets Console output with SourceContext for debugging.

**appsettings.json (default/production):**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "Properties": {
      "Application": "Platform"
    }
  }
}
```

**appsettings.Development.json:**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    },
    "Enrich": [ "FromLogContext", "WithMachineName" ],
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

### 5. SourceContext enricher for development

**Decision:** Include SourceContext in console output template

**Rationale:** Shows which class/service generated each log, essential for development debugging. Template format: `[Timestamp] [Level] [SourceContext] Message`

## Risks / Trade-offs

- **Silent production**: If no WriteTo configured, logs are discarded → Expected behavior, explicit opt-in for production logging
- **Missing Serilog package**: If package not restored, app fails to start → Mitigation: Ensure package in .csproj before any deployment
- **Configuration errors**: Invalid Serilog config causes silent failure → Mitigation: Add startup log entry to verify Serilog initialized
