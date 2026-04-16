## ADDED Requirements

### Requirement: Project dependencies must target .NET 10

所有项目文件 SHALL 指定 `net10.0` 作为目标框架，并使用与 .NET 10 兼容的 NuGet 包版本。

#### Scenario: All csproj files target net10.0
- **WHEN** examining all `.csproj` files in src/ and samples/
- **THEN** each SHALL have `<TargetFramework>net10.0</TargetFramework>`

#### Scenario: Package versions are .NET 10 compatible
- **WHEN** examining PackageReference versions in all projects
- **THEN** Npgsql.EntityFrameworkCore.PostgreSQL SHALL be version 10.x
- **AND** Microsoft.EntityFrameworkCore.Design SHALL be version 10.x
- **AND** Swashbuckle.AspNetCore SHALL be version 10.x
- **AND** Quartz SHALL be version 3.16.0 or higher
- **AND** YamlDotNet SHALL be version 16.x
- **AND** Microsoft.Extensions.Logging.Abstractions SHALL be version 10.x

### Requirement: Build must succeed after upgrade

升级依赖后，项目 SHALL 能够成功编译且无警告。

#### Scenario: Clean build succeeds
- **WHEN** executing `dotnet build` on all solutions
- **THEN** build SHALL complete with exit code 0
- **AND** there SHALL be no compilation warnings related to package version incompatibilities

### Requirement: Docker build must succeed

Dockerfile 中的基础镜像 SHALL 使用 .NET 10 镜像。

#### Scenario: Platform Dockerfile uses net10.0 base image
- **WHEN** examining `src/MinGo.Qap.Platform/Dockerfile`
- **THEN** base image SHALL be `mcr.microsoft.com/dotnet/aspnet:10.0`
- **AND** sdk image SHALL be `mcr.microsoft.com/dotnet/sdk:10.0`

#### Scenario: Agent Dockerfile uses net10.0 base image
- **WHEN** examining `src/MinGo.Qap.Agent/Dockerfile`
- **THEN** base image SHALL be `mcr.microsoft.com/dotnet/aspnet:10.0`
- **AND** sdk image SHALL be `mcr.microsoft.com/dotnet/sdk:10.0`

### Requirement: Breaking changes must be handled

升级过程中发现的 Breaking Changes SHALL 被识别并妥善处理。

#### Scenario: Npgsql date/time type handling
- **WHEN** upgrading to Npgsql 10.0
- **AND** application uses DateOnly/TimeOnly types for date columns
- **THEN** queries SHALL return correct DateOnly/TimeOnly values
- **OR** LegacyDateAndTimeResolver SHALL be configured if backward compatibility is required

#### Scenario: YamlDotNet type converters updated
- **WHEN** upgrading to YamlDotNet 16.x
- **AND** custom ITypeConverter implementations exist
- **THEN** those converters SHALL be updated to match the new interface signature
- **AND** SHALL compile without errors
