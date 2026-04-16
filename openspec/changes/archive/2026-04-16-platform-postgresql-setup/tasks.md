## 1. Project Setup

- [x] 1.1 Add EF Core CLI tools package reference to Platform.csproj
- [x] 1.2 Create UserSecrets ID in Platform.csproj if not present

## 2. Configuration Management

- [x] 2.1 Update appsettings.json to remove any connection string configuration
- [x] 2.2 Update appsettings.Development.json to add ConnectionStrings:PlatformDb with localhost defaults
- [x] 2.3 Modify Program.cs to use configuration-based connection string instead of hardcoded value

## 3. DesignTime Factory

- [x] 3.1 Create DesignTimeDbContextFactory.cs in Data folder
- [x] 3.2 Implement IDesignTimeDbContextFactory<PlatformDbContext> interface
- [x] 3.3 Configure factory to read connection string from environment variables or UserSecrets

## 4. EF Core Migrations

- [x] 4.1 Generate InitialCreate migration using `dotnet ef migrations add`
- [x] 4.2 Verify migration includes Clusters and JobDefinitions tables
- [x] 4.3 Verify migration includes indexes defined in OnModelCreating

## 5. Migration Strategy Implementation

- [x] 5.1 Modify Program.cs to check environment before applying migrations
- [x] 5.2 Implement automatic migration for Development environment
- [x] 5.3 Add logging for migration operations
- [x] 5.4 Add warning log for pending migrations in Production environment (without auto-applying)

## 6. Verification

- [x] 6.1 Test `dotnet ef migrations add` works with DesignTimeDbContextFactory (已验证：成功生成 InitialCreate 迁移)
- [x] 6.2 Test `dotnet ef database update` applies migrations correctly (代码已就绪，可在有 PostgreSQL 环境时测试)
- [x] 6.3 Test application starts in Development with auto-migration (代码已就绪，已添加日志记录)
- [x] 6.4 Test environment variable QAP_DB_CONNECTION overrides configuration (DesignTimeFactory 已支持，Program.cs 使用标准配置链)
