## ADDED Requirements

### Requirement: Toast displays success message
The toast notification system SHALL display success messages for completed operations.

#### Scenario: Success operation
- **WHEN** an operation completes successfully (e.g., job created, cluster deleted)
- **THEN** a green success toast SHALL appear
- **AND** SHALL display a checkmark icon
- **AND** SHALL display the success message
- **AND** SHALL auto-dismiss after 3 seconds

### Requirement: Toast displays error message
The toast notification system SHALL display error messages for failed operations.

#### Scenario: Operation fails
- **WHEN** an operation fails (e.g., API error, validation error)
- **THEN** a red error toast SHALL appear
- **AND** SHALL display an error icon
- **AND** SHALL display the error message
- **AND** SHALL NOT auto-dismiss (user must close)

### Requirement: Toast displays loading state
The toast notification system SHALL display loading messages for pending operations.

#### Scenario: Operation in progress
- **WHEN** an async operation is started (e.g., creating job)
- **THEN** a blue loading toast SHALL appear
- **AND** SHALL display a spinner
- **AND** SHALL display the loading message
- **AND** SHALL automatically transition to success or error when complete

### Requirement: Toast displays warning message
The toast notification system SHALL display warning messages for cautionary operations.

#### Scenario: Warning condition
- **WHEN** a potentially risky operation is attempted
- **THEN** a yellow warning toast SHALL appear
- **AND** SHALL display a warning icon
- **AND** SHALL display the warning message

### Requirement: Toast allows manual dismissal
The toast notification system SHALL allow users to manually dismiss any toast.

#### Scenario: Manual dismiss
- **WHEN** a toast is displayed
- **AND** user clicks the close (X) button
- **THEN** the toast SHALL be removed immediately

### Requirement: Toast stacks multiple notifications
The toast notification system SHALL display multiple toasts in a stacked format.

#### Scenario: Multiple toasts
- **WHEN** multiple operations trigger toasts
- **AND** toasts are generated in quick succession
- **THEN** toasts SHALL be stacked vertically
- **AND** SHALL NOT overlap or hide each other

### Requirement: Toast appears in consistent position
The toast notification system SHALL display toasts in a consistent screen position.

#### Scenario: Toast position
- **WHEN** any toast is triggered
- **THEN** the toast SHALL appear in the top-right corner of the screen
- **AND** SHALL have consistent padding and margins

### Requirement: Toast animates in and out
The toast notification system SHALL use smooth animations for appearing and disappearing.

#### Scenario: Animation
- **WHEN** toast appears
- **THEN** it SHALL slide in from the right
- **AND** WHEN toast disappears
- **THEN** it SHALL fade out

### Requirement: Toast handles promise automatically
The toast notification system SHALL automatically handle promise-based operations.

#### Scenario: Promise handling
- **WHEN** toast.promise() is called with a promise
- **THEN** the system SHALL:
  - Show loading state while promise is pending
  - Show success state when promise resolves
  - Show error state when promise rejects

### Requirement: Toast supports custom duration
The toast notification system SHALL allow custom display duration.

#### Scenario: Custom duration
- **WHEN** toast is called with a custom duration option
- **THEN** the toast SHALL respect the specified duration
- **AND** dismiss after that duration