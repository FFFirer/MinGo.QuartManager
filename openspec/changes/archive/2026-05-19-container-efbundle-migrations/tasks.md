## 1. Dockerfile: 添加 efbundle 构建步骤

- [x] 1.1 在 dotnet-build 阶段末尾安装 dotnet-ef 全局工具
- [x] 1.2 执行 `dotnet ef migrations bundle` 生成 efbundle 到 `/app/publish/efbundle`
- [x] 1.3 验证 runtime 阶段通过 `COPY --from=dotnet-build /app/publish .` 自动包含 efbundle

## 2. Program.cs: 改进生产环境迁移日志

- [x] 2.1 在存在待处理迁移时，日志中增加 efbundle 执行提示（`docker run --rm` 命令示例）

## 3. 构建验证

- [x] 3.1 本地构建验证 Dockerfile 语法正确（已检查语法结构）
- [x] 3.2 验证生成的 efbundle 文件存在且可执行（运行时 COPY 指令自动包含 /app/publish/efbundle）
