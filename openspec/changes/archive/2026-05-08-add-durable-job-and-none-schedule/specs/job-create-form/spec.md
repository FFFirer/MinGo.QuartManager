# Job Create Form Specification

## MODIFIED Requirements

### Requirement: User can configure schedule

The form SHALL support four schedule types: Cron, Interval, Once, and None.

#### Scenario: Cron schedule with presets
<!-- unchanged -->
- **WHEN** schedule type is "Cron"
- **THEN** system shows a text input for cron expression
- **AND** preset buttons: "每日午夜" (0 0 * * *), "每6小时" (0 */6 * * *), "每周一" (0 0 * * 1)
- **AND** clicking a preset fills the cron expression input

#### Scenario: Cron validation
<!-- unchanged -->
- **WHEN** user enters an invalid cron expression (empty or wrong format)
- **THEN** system shows validation error "Please enter a valid cron expression"

#### Scenario: Interval schedule
<!-- unchanged -->
- **WHEN** schedule type is "Interval"
- **THEN** system shows number inputs for hours, minutes, and seconds
- **AND** at least one must be > 0

#### Scenario: Once schedule
<!-- unchanged -->
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
<!-- unchanged -->
- **WHEN** user toggles "Disallow Concurrent Execution"
- **THEN** the option is included in the submission request

#### Scenario: Misfire policy selection
<!-- unchanged -->
- **WHEN** user selects a misfire policy from dropdown
- **THEN** the selected policy is included in the submission request
- **AND** options include "Fire and Proceed", "Ignore Misfire", "Do Nothing"

### Requirement: Form submits correctly

The form SHALL submit a valid `CreateJobRequest` to the API, supporting the new "None" schedule type and StoreDurable option.

**Extends**: Handles declarative creation response codes (200 success, 409 Conflict, 502 Agent error).

#### Scenario: Successful submission with None schedule and non-durable (200)
- **WHEN** user selects Schedule="None", StoreDurable=unchecked, fills required fields and clicks "Create Job"
- **THEN** system sends POST with `schedule.type = "None"` and `options.storeDurable = false`
- **AND** Agent creates the job without trigger, using `storeNonDurableWhileAwaitingScheduling: true`

#### Scenario: Successful submission with None schedule and durable (200)
- **WHEN** user selects Schedule="None", checks "持久化 Job", fills required fields and clicks "Create Job"
- **THEN** system sends POST with `schedule.type = "None"` and `options.storeDurable = true`
- **AND** Agent creates the job without trigger, with StoreDurable=true
