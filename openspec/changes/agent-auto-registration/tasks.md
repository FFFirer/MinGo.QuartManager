# Tasks: Agent 自动注册及相关能力

## 1. 核心实现（已部分完成）

- [ ] 1.1 检查 Platform 注册端点实现 - `POST /api/clusters/{clusterId}/agents`
- [ ] 1.2 检查 Platform 心跳端点实现 - `POST /api/agents/{agentId}/heartbeat`
- [ ] 1.3 检查 AgentRegistrationService (Agent端) 实现
- [ ] 1.4 检查 HeartbeatService (Agent端) 实现
- [ ] 1.5 验证 Token 哈希验证逻辑

## 2. 数据模型

- [ ] 2.1 检查 AgentInstance 实体字段完整性
- [ ] 2.2 检查 CreateAgentRequest / AgentRegistrationResponse DTO
- [ ] 2.3 检查 AgentHeartbeatRequest / AgentHeartbeatResponse DTO

## 3. 测试验证

- [ ] 3.1 Agent 启动自动注册测试
- [ ] 3.2 心跳上报测试
- [ ] 3.3 健康状态计算测试 (Online/Warning/Offline)
- [ ] 3.4 重复注册处理测试
- [ ] 3.5 Token 认证失败测试

## 4. 文档补充

- [ ] 4.1 更新 API Reference 文档
- [ ] 4.2 补充 Token 配置说明
- [ ] 4.3 补充故障排查指南

## 5. 可选增强

- [ ] 5.1 配置化阈值（从配置文件读取 Warning/Offline 阈值）
- [ ] 5.2 心跳 Metrics 字段扩展
- [ ] 5.3 Token 轮换接口