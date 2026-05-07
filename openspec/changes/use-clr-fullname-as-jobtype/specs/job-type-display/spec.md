## ADDED Requirements

### Requirement: JobType display component
The system SHALL provide a UI component that displays job type full names with truncation, tooltip, and copy functionality.

#### Scenario: Default truncated display
- **WHEN** a job type full name (e.g., `Sample.Agent.Jobs.HelloJob`) is rendered
- **THEN** the display SHALL show the last segment prominently (e.g., `HelloJob`) with the namespace prefix in a lighter color
- **AND** if the full name fits without overflow, it SHALL NOT be truncated

#### Scenario: Hover tooltip
- **WHEN** user hovers over the job type display
- **THEN** a tooltip SHALL appear showing the complete full name

#### Scenario: Copy to clipboard
- **WHEN** user clicks the copy button next to the job type
- **THEN** the full CLR type name SHALL be copied to clipboard
- **AND** a brief "Copied" feedback SHALL be shown
