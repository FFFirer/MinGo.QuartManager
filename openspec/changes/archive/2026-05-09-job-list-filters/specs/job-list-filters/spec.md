## ADDED Requirements

### Requirement: Job list Group text filter
The system SHALL provide a text input field to filter the job list by Group name. The filter value SHALL be sent as the `group` query parameter to the backend API.

#### Scenario: Filter by exact group name
- **WHEN** user types "DEFAULT" in the Group filter input
- **THEN** the job list SHALL only display jobs whose group matches "DEFAULT"
- **THEN** the page SHALL reset to page 1

#### Scenario: Filter by partial group name
- **WHEN** user types "My" in the Group filter input
- **THEN** the job list SHALL only display jobs whose group contains "My"

### Requirement: Job list Name (keyword) text filter with debounce
The system SHALL provide a text input field to filter the job list by Name/keyword. The filter value SHALL be sent as the `keyword` query parameter. Input SHALL have a 500ms debounce before triggering the API request.

#### Scenario: Filter by keyword with debounce
- **WHEN** user types "Report" in the Name filter input
- **THEN** the job list SHALL only display jobs whose key contains "Report"
- **WHEN** user continues typing within 500ms
- **THEN** the system SHALL NOT send additional API requests until 500ms after the last keystroke

#### Scenario: Empty filter shows all
- **WHEN** user clears the Group and Name filter inputs
- **THEN** the job list SHALL display all jobs for the scheduler

### Requirement: Filter state persisted in URL
The system SHALL persist Group and Name filter values in URL query parameters (`group` and `name`), enabling link sharing and browser back/forward navigation.

#### Scenario: URL reflects filter state
- **WHEN** user enters "DEFAULT" in Group and "Report" in Name
- **THEN** the URL SHALL contain `?group=DEFAULT&name=Report`
- **WHEN** user navigates to a bookmarked URL with `?group=MyGroup&name=Job1`
- **THEN** the filter inputs SHALL be pre-populated with "MyGroup" and "Job1"
- **THEN** the job list SHALL filter accordingly

### Requirement: Filter change resets to page 1
The system SHALL reset the current page to 1 whenever Group or Name filter values change.

#### Scenario: Filter change resets page
- **WHEN** user is on page 3 of the job list
- **WHEN** user types in the Group filter
- **THEN** the page SHALL reset to page 1
