## ADDED Requirements

### Requirement: Agent auto-registers on startup

The system SHALL automatically register the Agent instance with the Platform when the application starts, before sending any heartbeats.

#### Scenario: Successful registration on startup
- **WHEN** the application starts
- **THEN** `HostedAgentService` calls `IAgentRegistrationService.RegisterAsync()`
- **THEN** on success, the registration info (AgentId, PlatformApiBaseUrl, HeartbeatIntervalSeconds) is stored in memory
- **THEN** the heartbeat loop begins

#### Scenario: Registration retries on failure
- **WHEN** registration fails (network error, server error)
- **THEN** the service retries after `RegistrationRetryDelaySeconds` (default 5s)
- **THEN** retries up to `RegistrationMaxAttempts` (default 5) times
- **THEN** if all attempts fail, a warning is logged and the heartbeat loop is skipped

#### Scenario: Registration cancelled during shutdown
- **WHEN** cancellation is requested during registration retry
- **THEN** the service stops retrying immediately
- **THEN** the application continues shutdown without blocking

### Requirement: Agent sends periodic heartbeats

After successful registration, the system SHALL send heartbeats to the Platform at the configured interval to indicate the Agent is alive and report scheduler status.

#### Scenario: Heartbeat sent at configured interval
- **WHEN** registration is complete
- **THEN** a heartbeat is sent every `HeartbeatIntervalSeconds` (from registration response, or config, or default 30s)
- **THEN** the heartbeat includes scheduler status, job counts, process uptime, and memory usage

#### Scenario: Heartbeat interval is dynamic
- **WHEN** the Platform returns a different `HeartbeatIntervalSeconds` in the registration response
- **THEN** the service updates its heartbeat interval to the new value
- **THEN** subsequent heartbeats are sent at the new interval

#### Scenario: Heartbeat succeeds
- **WHEN** the Platform responds with `success=true`
- **THEN** the service logs heartbeat success at Debug level
- **THEN** the registration is considered valid

#### Scenario: Heartbeat fails due to auth or not-found
- **WHEN** the Platform responds with 401 (Unauthorized) or 404 (Not Found)
- **THEN** a warning is logged
- **THEN** the service triggers re-registration

#### Scenario: Heartbeat fails due to network error
- **WHEN** a network error occurs during heartbeat
- **THEN** an error is logged
- **THEN** the service waits for the next heartbeat interval
- **THEN** after 3 consecutive failures, the service triggers re-registration

### Requirement: Agent deregisters on shutdown

The system SHALL gracefully deregister the Agent instance when the application stops.

#### Scenario: Successful deregistration on graceful shutdown
- **WHEN** the application is shutting down (ASP.NET Core host stops)
- **THEN** `HostedAgentService.StopAsync` is called
- **THEN** `IAgentRegistrationService.DeregisterAsync()` is called
- **THEN** on success, a confirmation is logged

#### Scenario: Deregistration failure on shutdown
- **WHEN** deregistration fails (network error, server error)
- **THEN** an error is logged
- **THEN** the shutdown continues without blocking

#### Scenario: No deregistration if never registered
- **WHEN** the application shuts down but registration never succeeded
- **THEN** deregistration is skipped
- **THEN** a debug log is written

### Requirement: HostedAgentService is auto-registered in DI

The system SHALL automatically register `HostedAgentService` as a hosted service when `AddMinGoAgent()` is called.

#### Scenario: HostedAgentService registered via AddMinGoAgent
- **WHEN** the host application calls `builder.Services.AddMinGoAgent(builder.Configuration)`
- **THEN** `HostedAgentService` is added to the DI container as a hosted service
- **THEN** the service starts automatically with the application host

### Requirement: HostedAgentService handles re-registration after heartbeat failure

When the registration becomes invalid (e.g., Platform restarted), the system SHALL re-register automatically.

#### Scenario: Re-registration triggered by heartbeat failure
- **WHEN** heartbeat returns 401 or 404
- **THEN** the service immediately triggers re-registration
- **THEN** if re-registration succeeds, heartbeat resumes with new registration info
- **THEN** if re-registration fails, the service retries with the standard retry policy

#### Scenario: Multiple consecutive heartbeat failures before re-registration
- **WHEN** 3 consecutive heartbeats fail due to network issues
- **THEN** the service triggers re-registration
- **THEN** the heartbeat failure counter resets on successful heartbeat or registration
