# MinGo.Qap 集成测试计划

## 已完成的工作

### 1. 核心功能实现
- ✅ **共享模型**：实现了所有枚举和DTO，包括验证属性
- ✅ **Agent配置系统**：支持YAML配置和环境变量覆盖
- ✅ **Job注册表系统**：实现了作业类型注册和manifest端点
- ✅ **Job转换器**：支持三种调度类型（Once、Cron、Interval）
- ✅ **Quartz服务**：实现了作业的CRUD操作和控制器端点
- ✅ **心跳服务**：定期向Platform上报状态
- ✅ **Agent代理服务**：处理Platform到Agent的请求转发
- ✅ **Job服务**：实现了作业的管理和状态同步
- ✅ **Manifest控制器**：处理作业类型清单的存储和获取

### 2. 项目结构
- `MinGo.Qap.Shared`：共享模型和工具类
- `MinGo.Qap.Platform`：平台服务，管理集群和作业
- `MinGo.Qap.Agent`：代理服务，执行作业和上报状态
- `MinGo.Qap.UI`：前端界面（部分实现）
- `Sample.Jobs`：示例作业

### 3. 部署配置
- 创建了 `docker-compose.yml` 文件，包含 Platform、Agent 和数据库
- 配置了环境变量和卷挂载

## 集成测试步骤

### 1. 环境准备
1. 安装 Docker Desktop
2. 安装 .NET 8.0 SDK
3. 安装 Node.js 20.19+（用于前端构建）

### 2. 构建和启动
1. 构建所有项目：
   ```bash
   dotnet build MinGo.Qap.slnx -c Release
   ```

2. 构建前端：
   ```bash
   cd src/MinGo.Qap.UI && npm install && npm run build
   ```

3. 启动服务：
   ```bash
   docker-compose up -d
   ```

### 3. 测试端点

#### Agent 服务
- **健康检查**：`GET http://localhost:8080/health`
- **Job Manifest**：`GET http://localhost:8080/api/jobs/manifest`
- **作业列表**：`GET http://localhost:8080/api/jobs`
- **创建作业**：`POST http://localhost:8080/api/jobs`
- **触发作业**：`POST http://localhost:8080/api/jobs/{jobKey}/trigger`

#### Platform 服务
- **集群列表**：`GET http://localhost:5000/api/clusters`
- **创建集群**：`POST http://localhost:5000/api/clusters`
- **作业管理**：`GET http://localhost:5000/api/clusters/{clusterId}/jobs`
- **Manifest存储**：`POST http://localhost:5000/api/clusters/{clusterId}/manifest`

### 4. 测试场景

#### 场景1：创建集群
1. 调用 Platform 的创建集群接口
2. 获取返回的 token
3. 配置 Agent 使用该 token
4. 启动 Agent
5. 检查 Platform 是否收到心跳

#### 场景2：创建和执行作业
1. 调用 Agent 的 manifest 接口，获取可用的作业类型
2. 调用 Platform 的创建作业接口
3. 检查 Agent 是否接收到作业
4. 触发作业执行
5. 检查作业执行结果

#### 场景3：集群状态监控
1. 启动多个 Agent
2. 停止其中一个 Agent
3. 检查 Platform 是否正确标记该 Agent 为离线
4. 重启 Agent
5. 检查 Platform 是否正确标记该 Agent 为在线

### 5. 故障恢复测试
1. 停止 Platform 服务
2. 让 Agent 继续运行
3. 重启 Platform 服务
4. 检查 Agent 是否重新连接
5. 检查作业状态是否同步

## 预期结果

- 所有服务正常启动
- 集群和作业管理功能正常
- 心跳机制正常工作
- 故障恢复机制正常
- 前端界面能够正常访问和操作

## 注意事项

- 确保数据库连接字符串正确
- 确保 Agent 配置文件中的 Platform URL 正确
- 确保防火墙允许服务之间的通信
- 首次启动时，Platform 会自动迁移数据库

## 后续优化

- 添加更多的单元测试和集成测试
- 优化日志记录和监控
- 增加更多的作业类型和示例
- 完善前端界面的功能
- 优化性能和可靠性
