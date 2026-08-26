# Custom OpenTelemetry Telemetry Specification

## Purpose

This specification defines the custom OpenTelemetry (OTel) metrics, traces, and resource attributes that extend the existing auto-instrumentation in both Platform and Agent components of MinGo QAP.

**Status:** New  
**Last Updated:** 2026-08-26

---

## Background

The Platform already integrates OTel with auto-instrumentation for ASP.NET Core, HttpClient, EF Core, and Runtime metrics. However, there is no custom business-level telemetry to observe domain-specific operations such as agent registration, heartbeat processing, proxy forwarding, job declarations, and execution log ingestion. The Agent component has no OTel integration at all.

---

## Requirements

### Requirement: Shared telemetry infrastructure class

A shared `QapTelemetry` static class SHALL be defined in `MinGo.Qap.Shared` to centralize all `ActivitySource` and `Meter` definitions.

#### Scenario: ActivitySource definition
- **WHEN** any component creates a distributed trace span
- **THEN** it SHALL use `QapTelemetry.ActivitySource` with name `"MinGo.Qap"`

#### Scenario: Meter definition
- **WHEN** any component records a metric
- **THEN** it SHALL use `QapTelemetry.Meter` with name `"MinGo.Qap"`

---

### Requirement: Platform custom metrics — Counters

The Platform SHALL define and increment the following counters:

| Metric Name | Description | Tags |
|-------------|-------------|------|
| `qap.agent.registrations` | Total agent registrations | `type` (new/reconnect) |
| `qap.agent.heartbeats` | Total heartbeats processed | `agent.id` |
| `qap.proxy.requests` | Total proxy forwarding requests | `scheduler.name`, `http.method` |
| `qap.proxy.errors` | Total proxy forwarding failures | `scheduler.name`, `error.code` |
| `qap.jobs.declared` | Total job declarations | `scheduler.name`, `status` (synced/failed) |
| `qap.jobs.batch_operations` | Total batch operations | `action`, `scheduler.name` |
| `qap.logs.received` | Total execution logs received | `agent.id` |
| `qap.cache.hits` | Manifest cache hits | `scheduler.name` |
| `qap.cache.misses` | Manifest cache misses | `scheduler.name` |
| `qap.cache.invalidations` | Manifest cache invalidations | `reason` |

#### Scenario: Agent registration counter
- **WHEN** `AgentService.RegisterAsync` completes successfully
- **THEN** `qap.agent.registrations` SHALL be incremented
- **AND** tag `type` SHALL be `"new"` or `"reconnect"` accordingly

#### Scenario: Heartbeat counter
- **WHEN** `AgentService.UpdateHeartbeatAsync` is called
- **THEN** `qap.agent.heartbeats` SHALL be incremented with tag `agent.id`

#### Scenario: Proxy request counter
- **WHEN** `AgentProxyService` sends a request to an Agent
- **THEN** `qap.proxy.requests` SHALL be incremented
- **AND** tags SHALL include `scheduler.name` and `http.method`

#### Scenario: Proxy error counter
- **WHEN** `AgentProxyService` request fails (non-success status or exception)
- **THEN** `qap.proxy.errors` SHALL be incremented with `error.code` tag

#### Scenario: Job declaration counter
- **WHEN** `JobService.CreateAsync` completes
- **THEN** `qap.jobs.declared` SHALL be incremented with `status` = `"synced"` or `"failed"`

#### Scenario: Execution log received counter
- **WHEN** `ExecutionLogService.ReceiveLogsAsync` processes logs
- **THEN** `qap.logs.received` SHALL be incremented by the number of logs received

---

### Requirement: Platform custom metrics — Histograms

The Platform SHALL define and record the following histograms:

| Metric Name | Description | Unit | Tags |
|-------------|-------------|------|------|
| `qap.proxy.duration` | Proxy forwarding latency | ms | `scheduler.name`, `http.method` |
| `qap.job.declare.duration` | Job declaration end-to-end latency | ms | `scheduler.name` |
| `qap.agent.route.duration` | Agent routing selection latency | ms | `scheduler.name` |
| `qap.logs.batch_size` | Number of logs per batch | `{count}` | `agent.id` |

#### Scenario: Proxy duration histogram
- **WHEN** `AgentProxyService` completes a forwarding request
- **THEN** `qap.proxy.duration` SHALL record the elapsed time in milliseconds

#### Scenario: Job declare duration histogram
- **WHEN** `JobService.CreateAsync` completes (success or failure)
- **THEN** `qap.job.declare.duration` SHALL record the elapsed time in milliseconds

#### Scenario: Logs batch size histogram
- **WHEN** `ExecutionLogService.ReceiveLogsAsync` receives a batch
- **THEN** `qap.logs.batch_size` SHALL record the number of logs in the batch

---

### Requirement: Platform custom metrics — Observable Gauges

The Platform SHALL define and observe the following gauges:

| Metric Name | Description | Source |
|-------------|-------------|--------|
| `qap.agents.online` | Current online agent count | DB query `Status == "Online"` |
| `qap.agents.warning` | Current warning-state Agent count | DB query `Status == "Warning"` |
| `qap.agents.offline` | Current offline Agent count | DB query `Status == "Offline"` |
| `qap.cache.entries` | Current manifest cache entry count | `ManifestCacheService` internal count |
| `qap.jobs.pending_sync` | Pending-sync job declarations | DB query `Status == Pending` |
| `qap.jobs.failed_sync` | Failed-sync job declarations | DB query `Status == Failed` |

#### Scenario: Agent status gauges
- **WHEN** OTel metrics collection callback fires
- **THEN** `qap.agents.online/warning/offline` SHALL reflect current DB state

#### Scenario: Cache entries gauge
- **WHEN** OTel metrics collection callback fires
- **THEN** `qap.cache.entries` SHALL reflect the current count of cached manifests

---

### Requirement: Platform custom traces — ActivitySource spans

The Platform SHALL create custom `Activity` spans for key business operations using `QapTelemetry.ActivitySource`.

| Span Name | Service | Key Tags |
|-----------|---------|----------|
| `qap.agent.register` | AgentService | `agent.id`, `is_reconnect`, `agent.version` |
| `qap.proxy.forward` | AgentProxyService | `scheduler.name`, `agent.id`, `http.method`, `path` |
| `qap.job.declare` | JobService | `job.key`, `scheduler.name`, `sync.status` |
| `qap.job.batch` | JobService | `action`, `scheduler.name`, `total`, `successes`, `failures` |
| `qap.scheduler.report` | SchedulerService | `agent.id`, `scheduler.count` |
| `qap.logs.receive` | ExecutionLogService | `agent.id`, `log.count` |

#### Scenario: Agent registration span
- **WHEN** `AgentService.RegisterAsync` is called
- **THEN** a span `qap.agent.register` SHALL be started
- **AND** tags `agent.id`, `is_reconnect` SHALL be set
- **AND** the span SHALL be stopped when the method completes

#### Scenario: Proxy forward span
- **WHEN** any `AgentProxyService` method (Get/Post/Put/Delete) is called
- **THEN** a span `qap.proxy.forward` SHALL be started
- **AND** tags `scheduler.name`, `http.method`, `path` SHALL be set
- **AND** if the request fails, the span SHALL record the error

#### Scenario: Job declare span
- **WHEN** `JobService.CreateAsync` is called
- **THEN** a span `qap.job.declare` SHALL be started
- **AND** tags `job.key`, `scheduler.name` SHALL be set
- **AND** tag `sync.status` SHALL be `"synced"` or `"failed"` on completion

#### Scenario: Batch operation span
- **WHEN** `JobService.BatchAsync` is called
- **THEN** a span `qap.job.batch` SHALL be started
- **AND** tags `action`, `total` SHALL be set
- **AND** tags `successes`, `failures` SHALL be set on completion

---

### Requirement: Agent OTel integration

The Agent SHALL integrate with OpenTelemetry via `AgentExtensions.AddMinGoAgent()`, providing custom metrics and traces.

#### Scenario: Agent Meter registration
- **WHEN** `AddMinGoAgent` is called
- **THEN** the Agent SHALL use `QapTelemetry.Meter` for all custom metrics

#### Scenario: Agent ActivitySource usage
- **WHEN** the Agent creates trace spans
- **THEN** it SHALL use `QapTelemetry.ActivitySource`

---

### Requirement: Agent custom metrics — Counters

The Agent SHALL define and increment the following counters:

| Metric Name | Description | Tags |
|-------------|-------------|------|
| `qap.heartbeats.sent` | Total heartbeats sent | `agent.id` |
| `qap.heartbeats.failed` | Total heartbeat failures | `agent.id` |
| `qap.reregistrations` | Total re-registrations | `agent.id` |
| `qap.logs.flushed` | Total execution logs flushed to platform | `agent.id` |
| `qap.logs.flush_failed` | Total log flush failures | `agent.id` |

---

### Requirement: Agent custom metrics — Histograms

The Agent SHALL define and record the following histograms:

| Metric Name | Description | Unit | Tags |
|-------------|-------------|------|------|
| `qap.heartbeat.duration` | Heartbeat round-trip latency | ms | `agent.id` |
| `qap.logs.flush.duration` | Log flush round-trip latency | ms | `agent.id` |

---

### Requirement: Agent custom metrics — Observable Gauges

The Agent SHALL define and observe the following gauges:

| Metric Name | Description | Source |
|-------------|-------------|--------|
| `qap.logs.buffered` | Current buffered log count | `LogCollectionService` internal queue |
| `qap.schedulers.managed` | Number of managed Schedulers | `IAgentSchedulerAccessor` |
| `qap.schedulers.running_jobs` | Currently executing jobs | `IScheduler.GetCurrentlyExecutingJobs()` |

---

### Requirement: Agent custom traces — ActivitySource spans

The Agent SHALL create custom `Activity` spans for key lifecycle operations.

| Span Name | Service | Key Tags |
|-----------|---------|----------|
| `qap.agent.register` | AgentRegistrationService | `agent.id`, `attempt` |
| `qap.heartbeat.send` | HostedAgentService | `agent.id`, `scheduler.count` |
| `qap.scheduler.report` | SchedulerReporterService | `agent.id`, `scheduler.count` |
| `qap.logs.flush` | LogCollectionService | `agent.id`, `log.count` |

---

### Requirement: OTel pipeline registration

Both Platform and Agent SHALL register the custom `ActivitySource` and `Meter` with the OTel SDK pipeline.

#### Scenario: Platform pipeline registration
- **WHEN** Platform OTel is configured in `Program.cs`
- **THEN** `WithTracing` SHALL include `.AddSource("MinGo.Qap")`
- **AND** `WithMetrics` SHALL include `.AddMeter("MinGo.Qap")`

#### Scenario: Agent pipeline registration
- **WHEN** Agent OTel is configured in `AddMinGoAgent`
- **THEN** the Agent SHALL configure OTel tracing with `.AddSource("MinGo.Qap")`
- **AND** metrics with `.AddMeter("MinGo.Qap")`

---

### Requirement: Resource attributes enhancement

Both Platform and Agent SHALL enrich OTel Resource with domain-specific attributes.

#### Scenario: Platform resource attributes
- **WHEN** Platform OTel resource is configured
- **THEN** it SHALL include existing attributes: `service.name`, `service.version`, `deployment.environment`
- **AND** no additional Platform-specific attributes are required at this time

#### Scenario: Agent resource attributes
- **WHEN** Agent OTel resource is configured
- **THEN** it SHALL include: `service.name` (agent name or ID), `service.version`
- **AND** SHOULD include `qap.agent.id` once registered

---

## Non-Goals

- This spec does NOT cover modifying the existing auto-instrumentation configuration
- This spec does NOT cover OTel log enrichment (already handled by existing ILogger pipeline)
- This spec does NOT cover custom OTel exporters or collectors setup
- This spec does NOT cover frontend (UI) telemetry

---

## Implementation Notes

- All metric/tag names follow OpenTelemetry naming conventions (dot-separated, lowercase)
- `ActivitySource` and `Meter` are defined once in `QapTelemetry` (Shared) and consumed by both Platform and Agent
- Observable gauges use callbacks to avoid polling overhead
- Histograms use `IDisposable` timer pattern for duration measurement
- All spans follow the `qap.` prefix convention for namespace isolation
