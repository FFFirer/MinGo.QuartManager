## ADDED Requirements

### Requirement: StatusBadge displays correct colors for cluster statuses
The StatusBadge component SHALL display the correct background color for cluster status values.

#### Scenario: Online cluster status
- **WHEN** the StatusBadge component receives status="Online"
- **THEN** it SHALL render with a green background (bg-green-500)

#### Scenario: Warning cluster status
- **WHEN** the StatusBadge component receives status="Warning"
- **THEN** it SHALL render with an amber background (bg-amber-500)

#### Scenario: Offline cluster status
- **WHEN** the StatusBadge component receives status="Offline"
- **THEN** it SHALL render with a slate background (bg-slate-500)

#### Scenario: Pending cluster status
- **WHEN** the StatusBadge component receives status="Pending"
- **THEN** it SHALL render with a blue background (bg-blue-500)

#### Scenario: Deleted cluster status
- **WHEN** the StatusBadge component receives status="Deleted"
- **THEN** it SHALL render with a red background (bg-red-500)

### Requirement: StatusBadge displays correct colors for job statuses
The StatusBadge component SHALL display the correct background color for job status values.

#### Scenario: Normal job status
- **WHEN** the StatusBadge component receives status="normal"
- **THEN** it SHALL render with a green background (bg-green-500)

#### Scenario: Paused job status
- **WHEN** the StatusBadge component receives status="paused"
- **THEN** it SHALL render with an amber background (bg-amber-500)

#### Scenario: Blocked job status
- **WHEN** the StatusBadge component receives status="blocked"
- **THEN** it SHALL render with a red background (bg-red-500)

### Requirement: StatusBadge displays correct colors for agent statuses
The StatusBadge component SHALL display the correct background color for agent status values.

#### Scenario: Online agent status
- **WHEN** the StatusBadge component receives status="Online"
- **THEN** it SHALL render with a green background (bg-green-500)

#### Scenario: Warning agent status
- **WHEN** the StatusBadge component receives status="Warning"
- **THEN** it SHALL render with an amber background (bg-amber-500)

#### Scenario: Offline agent status
- **WHEN** the StatusBadge component receives status="Offline"
- **THEN** it SHALL render with a slate background (bg-slate-500)

#### Scenario: Pending agent status
- **WHEN** the StatusBadge component receives status="Pending"
- **THEN** it SHALL render with a blue background (bg-blue-500)

#### Scenario: Deleted agent status
- **WHEN** the StatusBadge component receives status="Deleted"
- **THEN** it SHALL render with a red background (bg-red-500)

### Requirement: StatusBadge supports different sizes
The StatusBadge component SHALL support different size variants for the status indicator.

#### Scenario: Small size
- **WHEN** the StatusBadge component receives size="sm"
- **THEN** it SHALL render the status dot with 8px width and height

#### Scenario: Medium size
- **WHEN** the StatusBadge component receives size="md"
- **THEN** it SHALL render the status dot with 12px width and height

#### Scenario: Large size
- **WHEN** the StatusBadge component receives size="lg"
- **THEN** it SHALL render the status dot with 16px width and height

### Requirement: StatusBadge supports different variants
The StatusBadge component SHALL support different visual variants for displaying status.

#### Scenario: Dot variant
- **WHEN** the StatusBadge component receives variant="dot" (or is not specified)
- **THEN** it SHALL render only the colored dot indicator

#### Scenario: Badge variant
- **WHEN** the StatusBadge component receives variant="badge"
- **THEN** it SHALL render the colored dot with the status text label

#### Scenario: Inline variant
- **WHEN** the StatusBadge component receives variant="inline"
- **THEN** it SHALL render the status text label with appropriate text color

### Requirement: StatusBadge shows/hides label based on prop
The StatusBadge component SHALL conditionally show the status label based on the showLabel prop.

#### Scenario: Show label
- **WHEN** the StatusBadge component receives showLabel=true
- **THEN** it SHALL render the status text label when variant supports it

#### Scenario: Hide label
- **WHEN** the StatusBadge component receives showLabel=false
- **THEN** it SHALL NOT render the status text label, showing only the dot indicator