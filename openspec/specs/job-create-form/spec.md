# Job Create Form Specification

## Purpose

This specification defines the full-page Job creation form, including group selection, job type selection, parameter editing, schedule configuration, and options.

**Status:** New  
**Last Updated:** 2026-05-07

---

## Requirements

### Requirement: User can navigate to full-page Create Job form
The system SHALL provide a dedicated route `/schedulers/{schedulerName}/jobs/create` for creating jobs in a full-page layout.

#### Scenario: Navigate from Jobs list
- **WHEN** user clicks "Create Job" button on JobsPage
- **THEN** system navigates to `/schedulers/{schedulerName}/jobs/create`

#### Scenario: Navigate with copyFrom parameter
- **WHEN** user clicks "Copy" action on a job row in JobsPage
- **THEN** system navigates to `/schedulers/{schedulerName}/jobs/create?copyFrom={GROUP.name}`

#### Scenario: Create page has back navigation
- **WHEN** user is on the Create Job page
- **THEN** system shows a "← Back to Jobs" link that navigates back to the Jobs list

### Requirement: User can select or enter Job Group
The form SHALL provide a Group field that allows selecting from existing groups or entering a custom group name.

#### Scenario: Group dropdown shows existing groups
- **WHEN** Create Job page loads
- **THEN** system fetches existing jobs and extracts unique group names
- **AND** populates a dropdown with those groups plus "DEFAULT" plus "Create New" option

#### Scenario: User selects existing group
- **WHEN** user selects a group from the dropdown
- **THEN** the selected group value is used when submitting

#### Scenario: User creates a new group
- **WHEN** user selects "Create New" in the group dropdown
- **THEN** a text input appears for entering a custom group name

### Requirement: User can enter Job Name
The form SHALL provide a required text input for the job name.

#### Scenario: Job name is required
- **WHEN** user submits the form without entering a job name
- **THEN** system shows validation error "Job name is required"

#### Scenario: Job name cannot contain dots
- **WHEN** user enters a job name containing dots
- **THEN** system shows validation error "Job name cannot contain '.' character"
- **AND** submission is blocked

### Requirement: User can select Job Type from manifest
The form SHALL display available job types from the scheduler manifest for selection.

#### Scenario: Job types are loaded from manifest
- **WHEN** Create Job page loads
- **THEN** system fetches manifest via `manifestApi.get(schedulerName)`
- **AND** displays available job types in a dropdown-style selector

#### Scenario: User selects a job type
- **WHEN** user clicks on a job type in the selector
- **THEN** the item becomes highlighted (selected state)
- **AND** the parameters section renders with fields defined in the manifest for that job type

### Requirement: Parameters render with appropriate form controls
The form SHALL render parameter fields based on their type from the manifest.

#### Scenario: String parameter renders as text input
- **WHEN** a parameter has type "string"
- **THEN** it renders as a text input field

#### Scenario: Integer parameter renders as number input
- **WHEN** a parameter has type "int"
- **THEN** it renders as a number input field with step=1

#### Scenario: Boolean parameter renders as select
- **WHEN** a parameter has type "bool"
- **THEN** it renders as a select dropdown with "True" and "False" options

#### Scenario: Complex parameter renders as JSON textarea
- **WHEN** a parameter has a type other than "string", "int", or "bool"
- **THEN** it renders as a textarea with JSON validation (red border on invalid JSON)

### Requirement: Required parameters are validated
The form SHALL validate that all required parameters (marked with `required: true` in manifest) have values before submission.

#### Scenario: Required parameter missing shows error
- **WHEN** user submits the form with a required parameter left empty
- **THEN** system shows inline error below the empty required field
- **AND** submission is blocked

#### Scenario: Required parameter marked visually
- **WHEN** a parameter is required
- **THEN** its label has a red asterisk `*` suffix

### Requirement: User can configure schedule
The form SHALL support four schedule types: Cron, Interval, Once, and None.

#### Scenario: Cron schedule with presets
- **WHEN** schedule type is "Cron"
- **THEN** system shows a text input for cron expression
- **AND** preset buttons: "每日午夜" (0 0 * * *), "每6小时" (0 */6 * * *), "每周一" (0 0 * * 1)
- **AND** clicking a preset fills the cron expression input

#### Scenario: Cron validation
- **WHEN** user enters an invalid cron expression (empty or wrong format)
- **THEN** system shows validation error "Please enter a valid cron expression"

#### Scenario: Interval schedule
- **WHEN** schedule type is "Interval"
- **THEN** system shows number inputs for hours, minutes, and seconds
- **AND** at least one must be > 0

#### Scenario: Once schedule
- **WHEN** schedule type is "Once"
- **THEN** system shows a datetime-local input for execution time
- **AND** leaving it empty means run immediately

#### Scenario: None schedule hides trigger fields
- **WHEN** schedule type is "None"
- **THEN** system hides all trigger configuration fields (cron, interval, datetime)
- **AND** shows info text: "Job will be created without a trigger. Use 'Trigger' action to fire manually."

### Requirement: User can configure options
The form SHALL support Quartz options: StoreDurable, Disallow Concurrent Execution, and Misfire Policy.

#### Scenario: StoreDurable checkbox
- **WHEN** user is on the Create Job page
- **THEN** system shows a "持久化 Job" checkbox in the Options section
- **AND** it is unchecked by default

#### Scenario: Disallow concurrent execution toggle
- **WHEN** user toggles "Disallow Concurrent Execution"
- **THEN** the option is included in the submission request

#### Scenario: Misfire policy selection
- **WHEN** user selects a misfire policy from dropdown
- **THEN** the selected policy is included in the submission request
- **AND** options include "Fire and Proceed", "Ignore Misfire", "Do Nothing"

### Requirement: Copy from existing job
The form SHALL support pre-filling all fields from an existing job when `?copyFrom=GROUP.name` is provided.

#### Scenario: CopyFrom prefills form fields
- **WHEN** page loads with `?copyFrom=GROUP.name` query parameter
- **THEN** system fetches existing job via `jobApi.get(schedulerName, "GROUP.name")`
- **AND** pre-fills: group, name (extracted from jobKey), params, schedule, options
- **AND** user can modify any field before submitting

#### Scenario: CopyFrom shows source indicator
- **WHEN** form is pre-filled from copyFrom
- **THEN** system shows a notice "Copying from: GROUP.name"

### Requirement: Form submits correctly
The form SHALL submit a valid `CreateJobRequest` to the API.

**Extends**: Handles declarative creation response codes (200 success, 409 Conflict, 502 Agent error).

#### Scenario: Successful submission (200)
- **WHEN** user fills all required fields and clicks "Create Job"
- **THEN** system sends POST to `/api/schedulers/{name}/jobs` with assembled request body
- **AND** `jobKey` is composed as `"{group}.{name}"`
- **AND** on success (200), shows toast "Job created successfully!"
- **AND** navigates back to Jobs list

#### Scenario: Agent error on submission (502)
- **WHEN** Agent returns an error during job creation
- **THEN** system shows error toast with the Agent error message
- **AND** stays on the Create Job page

#### Scenario: Duplicate declaration (409)
- **WHEN** API returns HTTP 409 with message "Job已存在"
- **THEN** system shows warning toast "Job已存在，无需重复创建"
- **AND** stays on the Create Job page

#### Scenario: Successful submission with None schedule and non-durable (200)
- **WHEN** user selects Schedule="None", StoreDurable=unchecked, fills required fields and clicks "Create Job"
- **THEN** system sends POST with `schedule.type = "None"` and `options.storeDurable = false`
- **AND** Agent creates the job without trigger, using `storeNonDurableWhileAwaitingScheduling: true`

#### Scenario: Successful submission with None schedule and durable (200)
- **WHEN** user selects Schedule="None", checks "持久化 Job", fills required fields and clicks "Create Job"
- **THEN** system sends POST with `schedule.type = "None"` and `options.storeDurable = true`
- **AND** Agent creates the job without trigger, with StoreDurable=true
