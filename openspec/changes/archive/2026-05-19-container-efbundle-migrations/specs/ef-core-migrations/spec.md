## MODIFIED Requirements

### Requirement: Production environment must not auto-apply migrations

Platform SHALL NOT automatically apply migrations in Production environment.
生产环境 MUST NOT 在启动时自动迁移，但 MUST 提供通过 efbundle 执行迁移的能力。

#### Scenario: Production migration safety
- **WHEN** application starts in Production environment
- **AND** database has pending migrations
- **THEN** Platform SHALL NOT automatically apply them
- **AND** it SHALL log a warning about pending migrations
- **AND** it SHALL log instructions on how to run the included efbundle

#### Scenario: Production supports efbundle migration
- **WHEN** 运维人员执行 `docker run --rm -e ConnectionStrings__PlatformDb="..." <image> dotnet /app/efbundle.dll --connection "ConnectionStrings__PlatformDb"`
- **THEN** efbundle SHALL 应用所有待处理的迁移
- **AND** 返回退出码 0 表示成功
- **AND** 非零退出码表示失败，并输出错误信息
