## ADDED Requirements

### Requirement: Platform monitors agent instance health
The platform SHALL continuously monitor the health of all registered agent instances.

#### Scenario: Instance status calculation based on heartbeats
- **WHEN** an agent instance sends a heartbeat
- **THEN** the platform updates the instance's LastHeartbeat timestamp
- **AND** calculates status as:
  - **Online**: Last heartbeat within 30 seconds
  - **Warning**: Last heartbeat 30-60 seconds ago  
  - **Offline**: Last heartbeat more than 60 seconds ago
  - **Pending**: Never received a heartbeat

#### Scenario: Automated status updates
- **WHEN** the platform's status monitor runs periodically
- **THEN** it recalculates all instance statuses based on current time
- **AND** updates instance records with new status if changed
- **AND** logs status transitions for auditing

#### Scenario: Grace period for new instances
- **WHEN** a new agent instance registers
- **THEN** it starts in Pending status
- **AND** has a grace period (e.g., 90 seconds) to send its first heartbeat
- **AND** transitions to Offline if no heartbeat received within grace period

### Requirement: Platform calculates cluster status from instances
The platform SHALL calculate overall cluster status based on the status of its agent instances.

#### Scenario: Cluster status calculation algorithm
- **WHEN** determining a cluster's overall status
- **THEN** the platform uses the following algorithm:
  - **Online**: At least one instance is Online
  - **Warning**: No Online instances, but at least one Warning instance
  - **Offline**: All instances are Offline or no instances exist
  - **Pending**: All instances are Pending

#### Scenario: Real-time status updates
- **WHEN** any agent instance changes status
- **THEN** the platform recalculates the cluster status
- **AND** updates the cluster record if status changed
- **AND** triggers any configured alerts or notifications

### Requirement: Platform collects agent instance metrics
The platform SHALL collect and store metrics from agent instance heartbeats.

#### Scenario: System metrics collection
- **WHEN** an agent sends a heartbeat with system metrics
- **THEN** the platform stores:
  - Memory usage (used/total)
  - Uptime duration
  - Agent version
  - Timestamp of metrics collection

#### Scenario: Quartz metrics collection
- **WHEN** an agent sends a heartbeat with Quartz metrics
- **THEN** the platform stores:
  - Scheduler status (running/standby/shutdown)
  - Job counts by status (normal/paused/blocked)
  - Number of jobs executed
  - Misfire counts and handling

### Requirement: Platform provides instance health dashboards
The platform SHALL provide visibility into agent instance health through dashboards and APIs.

#### Scenario: Instance health summary
- **WHEN** viewing a cluster
- **THEN** the platform displays:
  - Total instance count
  - Count by status (Online/Warning/Offline/Pending)
  - Aggregate health indicators
  - Recent status changes

#### Scenario: Detailed instance health view
- **WHEN** drilling into instance health details
- **THEN** the platform provides:
  - Individual instance status and metrics
  - Heartbeat history and timing
  - Performance trends over time
  - Health events and transitions

### Requirement: Platform alerts on instance health issues
The platform SHALL provide alerting for agent instance health degradation.

#### Scenario: Warning threshold alert
- **WHEN** an instance transitions to Warning status
- **THEN** the platform generates a warning-level alert
- **AND** includes instance details and time since last heartbeat
- **AND** provides recovery suggestions

#### Scenario: Offline threshold alert
- **WHEN** an instance transitions to Offline status
- **THEN** the platform generates a critical-level alert
- **AND** includes cluster impact assessment
- **AND** suggests investigation steps

#### Scenario: Health recovery notification
- **WHEN** an instance recovers from Warning/Offline to Online
- **THEN** the platform generates an informational notification
- **AND** includes recovery time and duration of issue
- **AND** clears any related open alerts