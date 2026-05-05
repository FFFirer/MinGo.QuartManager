## ADDED Requirements

### Requirement: Refresh Jobs button

The scheduler Jobs page SHALL provide a "Refresh Jobs" button that re-fetches the current scheduler's job list from the API.

#### Scenario: Click refresh button refreshes job list
- **WHEN** user clicks the "Refresh Jobs" button on the scheduler Jobs page
- **THEN** the system re-fetches the job list from the API
- **THEN** the displayed jobs are updated with the latest data from the Agent

#### Scenario: Refresh button shows loading state
- **WHEN** user clicks the "Refresh Jobs" button
- **THEN** the button icon animates (spins) during the fetch
- **THEN** the button returns to normal state when the fetch completes

#### Scenario: Refresh preserves current page and filters
- **WHEN** user is on page 2 and clicks "Refresh Jobs"
- **THEN** the refreshed data maintains the current page and page size

### Requirement: Paginated job list response

The job list API SHALL return paginated results with total count for proper pagination display.

#### Scenario: Agent returns paged response
- **WHEN** frontend requests `/api/schedulers/{name}/jobs?page=1&pageSize=20`
- **THEN** the response contains `items` (array of jobs), `total` (total matching count), `page`, `pageSize`, `totalPages`

#### Scenario: Pagination bar displays correct total
- **WHEN** the job list has more items than the current page size
- **THEN** the pagination bar shows the correct total number of jobs and available pages

#### Scenario: Page navigation works correctly
- **WHEN** user clicks "Next" on the pagination bar
- **THEN** the frontend fetches the next page with correct `page` parameter
- **THEN** the table displays jobs for that page
