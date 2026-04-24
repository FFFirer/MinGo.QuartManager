# Specification: Job 完整生命周期管理

## Overview

本文档描述 Job 的完整生命周期管理操作。

**Target**: 开发/运维人员

---

## ADDED Requirements

### Requirement: 创建 Job

平台 SHALL 允许用户创建 Job。

#### Scenario: 通过 UI 创建 Job

- **WHEN** 用户在 UI 上填写 Job 表单
- **AND** 选择调度类型（Cron/Interval/Once）
- **AND** 点击 "Create"
- **THEN** Job 被创建到目标 Agent
- **AND** UI 显示成功消息

#### Scenario: 通过 API 创建 Job

- **WHEN** 用户调用 `POST /api/clusters/{clusterId}/jobs`
- **AND** 提供 Job 配置
- **THEN** Job 被创建
- **AND** 返回 Job 定义

### Requirement: 查看 Job 列表

平台 SHALL 允许用户列出 Job。

#### Scenario: 分页查看 Jobs

- **WHEN** 用户访问 Jobs 页面
- **AND** 可以分页浏览
- **THEN** 显示 Job 名称、状态、下次执行时间

#### Scenario: 过滤 Jobs

- **WHEN** 用户使用过滤条件
- **AND** 输入 group/status/keyword
- **THEN** 显示匹配的结果

### Requirement: 查看 Job 详情

平台 SHALL 允许用户查看 Job 详情。

#### Scenario: 查看单个 Job

- **WHEN** 用户点击 Job 名称
- **THEN** 显示完整配置信息
- **AND** 显示 Trigger 信息

### Requirement: 更新 Job

平台 SHALL 允许用户更新 Job。

#### Scenario: 通过 UI 更新 Job

- **WHEN** 用户编辑 Job 配置
- **AND** 保存更改
- **THEN** Job 配置更新

#### Scenario: 通过 API 更新 Job

- **WHEN** 用户调用 `PUT /api/clusters/{clusterId}/jobs/{jobKey}`
- **AND** 提供新配置
- **THEN** Job 更新

### Requirement: 删除 Job

平台 SHALL 允许用户删除 Job。

#### Scenario: 通过 UI 删除 Job

- **WHEN** 用户点击删除并确认
- **THEN** Job 被删除

#### Scenario: 通过 API 删除 Job

- **WHEN** 用户调用 `DELETE /api/clusters/{clusterId}/jobs/{jobKey}`
- **THEN** Job 被删除

### Requirement: 手动触发 Job

平台 SHALL 允许用户手动触发 Job。

#### Scenario: 通过 UI 手动触发

- **WHEN** 用户点击 "Trigger Now"
- **THEN** Job 立即执行一次

#### Scenario: 通过 API 手动触发

- **WHEN** 用户调用 `POST /api/clusters/{clusterId}/jobs/{jobKey}/trigger`
- **THEN** Job 立即执行

### Requirement: 暂停/恢复 Job

平台 SHALL 允许用户暂停和恢复 Job。

#### Scenario: 通过 UI 暂停 Job

- **WHEN** 用户点击 "Pause"
- **THEN** Job 暂停执行
- **AND** 状态显示 "Paused"

#### Scenario: 通过 UI 恢复 Job

- **WHEN** 用户点击 "Resume"
- **THEN** Job 恢复执行
- **AND** 状态显示 "Normal"

---

## Implementation Notes

- Job 配置包含：调度类型、表达式、Group、Description
- Cron 表达式需验证有效性
- Pause/Resume 使用 Quartz 原生 API