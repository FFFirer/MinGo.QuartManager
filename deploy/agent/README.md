# MinGo.Qap Agent 部署配置

## 快速开始

1. 复制示例配置
   ```bash
   cp config.yaml.example config.yaml
   ```

2. 编辑 `config.yaml`，设置你的配置：
   - `agent.clusterId`: 从 Platform 获取的 Cluster ID
   - `platform.url`: Platform API 地址
   - `quartz.jobTypes`: 你的 Job 类型列表

3. 放置 Job DLL 到 `jobs/` 目录

4. 启动 Agent
   ```bash
   docker-compose up -d
   ```

## 环境变量

可以通过环境变量覆盖配置：

| 变量 | 说明 | 默认值 |
|------|------|--------|
| `CLUSTER_ID` | Cluster ID | - |
| `PLATFORM_URL` | Platform API URL | http://localhost:5000 |
| `AGENT_PORT` | 宿主机端口 | 8080 |
| `LOG_LEVEL` | 日志级别 | Information |

## 配置文件

`config.yaml` 示例：

```yaml
agent:
  clusterId: cls-001
  port: 8080

platform:
  url: http://platform:5000

quartz:
  assemblyPath: ./jobs
  jobTypes:
    - MyApp.Jobs.InventorySyncJob
  properties:
    quartz.scheduler.instanceName: AgentScheduler
    quartz.jobStore.type: Quartz.Simpl.RAMJobStore, Quartz
```
