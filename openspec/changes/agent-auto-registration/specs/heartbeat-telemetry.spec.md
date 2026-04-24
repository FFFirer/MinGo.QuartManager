## ADDED Requirements

### Requirement: Periodic heartbeat with health payload
Description: The agent must send periodic heartbeat messages to the Platform containing health status, basic metrics, and identity information to indicate liveness and current state.

#### Scenario: Regular heartbeat emission
- **WHEN** the agent is running and the heartbeat interval elapses
- **THEN** a heartbeat message is sent to the Platform containing registrationId, status, and key metrics (uptime, CPU/memory rough estimates)

### Requirement: Exponential backoff on heartbeat failure
Description: If heartbeat transmissions fail due to network or Platform unavailability, the agent should back off progressively and retry, without overwhelming the Platform.

#### Scenario: Heartbeat backoff on failure
- **WHEN** a heartbeat attempt fails
- **THEN** the agent applies an exponential backoff and retries until success or a maximum retry window is reached
