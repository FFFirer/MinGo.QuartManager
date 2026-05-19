## Context

当前 `Dockerfile` 三阶段构建（UI → .NET SDK → Runtime）中，dotnet-build 阶段仅执行 `dotnet publish`，未生成 EF Core 迁移 bundle。生产环境下 `Program.cs` 仅检查并警告待处理迁移，运维人员需要手动在部署环境中安装 .NET SDK + dotnet-ef 工具来执行迁移——这在容器化部署中不可行。

## Goals / Non-Goals

**Goals:**
- 在 Docker 镜像构建的 dotnet-build 阶段自动生成 efbundle（`dotnet ef migrations bundle`）
- 将 efbundle 纳入 runtime 镜像，可通过 `docker run --rm` 执行数据库迁移
- 最小化镜像体积增加
- 保持现有 Dockerfile 结构不变

**Non-Goals:**
- 不改变生产环境启动时自动迁移行为（仍遵循 spec：不自动迁移）
- 不创建独立迁移镜像（方案 B/C 留待未来）
- 不修改 CI/CD 流水线

## Decisions

### Decision 1: Framework-dependent 模式（非 self-contained）

| | Framework-dependent | Self-contained |
|---|---|---|
| 镜像体积增加 | ~5MB | ~50MB |
| 依赖 | 需要 runtime 镜像有 .NET 运行时 | 独立运行 |
| 适用场景 | Runtime 镜像已有 dotnet/aspnet | 无运行时的精简镜像（如 Alpine） |

**选择**: Framework-dependent（默认模式）。Runtime 镜像 `dotnet/aspnet:10.0` 已包含 .NET 运行时，efbundle 作为框架依赖的可执行文件即可正常工作。用 `dotnet /app/efbundle.dll --connection "..."` 执行。

### Decision 2: 在 dotnet-build 阶段末尾生成 efbundle

在 `dotnet publish` 之后、COPY 到 runtime 之前插入 bundle 生成步骤。这样：
- 复用了 SDK 镜像中的 .NET SDK + NuGet 缓存
- 不需要额外阶段
- 输出直接进入 `/app/publish` 目录，被现有 `COPY --from=dotnet-build /app/publish .` 自动覆盖

### Decision 3: 安装 dotnet-ef tool 为全局工具

```dockerfile
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
```

dotnet-ef 是 bundle 生成的前提。安装为全局工具对现有构建无侵入。

### Decision 4: 输出路径和命名

- 输出到 `--output /app/publish/efbundle`
- 运行时以 `dotnet /app/efbundle.dll` 执行（bundle 生成 framework-dependent 的 DLL + 原生 host）

## Risks / Trade-offs

- **[体积增加]** efbundle（framework-dependent）增加 ~5MB → 可接受，在可重复的迁移能力面前微不足道
- **[网络依赖]** 安装 dotnet-ef tool 需要网络 → 但构建阶段本身就需要 NuGet restore，网络已可用
- **[构建时间]** 增加 ~10-20s 构建时间 → 可接受
- **[架构绑定]** efbundle 是 linux-x64 原生二进制 → 如果将来切换到 arm64 构建需要调整 `-r` 参数
- **[二进制缓存失效]** efbundle 在每次 migration 变化时重新生成 → 利用 Docker layer caching，仅在 migration 文件变化时重建
