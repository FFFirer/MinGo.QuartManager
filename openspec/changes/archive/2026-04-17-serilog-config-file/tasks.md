## 1. Add Serilog.AspNetCore Package

- [x] 1.1 Add Serilog.AspNetCore NuGet package to MinGo.Qap.Platform.csproj

## 2. Configure Serilog in Program.cs

- [x] 2.1 Add `using Serilog;` import
- [x] 2.2 Add `builder.Host.UseSerilog()` after `var builder = WebApplication.CreateBuilder(args);`
- [x] 2.3 Add `Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();` before `var app = builder.Build();`
- [x] 2.4 Remove existing `ILogger<Program>` usage in migration section (Serilog provides it)

## 3. Update appsettings.json

- [x] 3.1 Replace `Logging` section with minimal `Serilog` section (no WriteTo)
- [x] 3.2 Add `MinimumLevel.Default: Information`
- [x] 3.3 Add `Properties.Application: Platform`

## 4. Update appsettings.Development.json

- [x] 4.1 Add `Serilog` section with `MinimumLevel.Default: Debug`
- [x] 4.2 Add `MinimumLevel.Override` for Microsoft and Microsoft.Hosting.Lifetime
- [x] 4.3 Add `Enrich: [FromLogContext, WithMachineName]`
- [x] 4.4 Add `WriteTo` array with Console sink and SourceContext template

## 5. Verify

- [x] 5.1 Run application in Development environment and verify Console output
- [x] 5.2 Verify SourceContext appears in log output
- [x] 5.3 Ensure no file output in development (only Console)
