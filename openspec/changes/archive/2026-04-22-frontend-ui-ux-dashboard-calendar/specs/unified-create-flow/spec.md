## ADDED Requirements

### Requirement: Create wizard follows 4-step pattern
The unified create flow SHALL follow a consistent 4-step wizard pattern for all resource creation.

#### Scenario: Create wizard steps
- **WHEN** user initiates any create operation (cluster, job)
- **THEN** the wizard SHALL display 4 steps:
  1. Basic Info (name, type selection)
  2. Configuration (parameters)
  3. Schedule/Settings (optional settings)
  4. Review & Confirm

### Requirement: Create wizard shows progress indicator
The create wizard SHALL display a visual progress indicator showing current step.

#### Scenario: Progress display
- **WHEN** create wizard is open
- **THEN** a horizontal progress bar SHALL be displayed
- **AND** SHALL show all 4 step indicators
- **AND** SHALL highlight the current step
- **AND** SHALL mark completed steps with checkmark

### Requirement: Create wizard validates each step
The create wizard SHALL validate input before allowing progression to next step.

#### Scenario: Step validation
- **WHEN** user clicks "Next" button
- **AND** current step has validation errors
- **THEN** the wizard SHALL NOT proceed to next step
- **AND** SHALL display error messages for invalid fields
- **AND** SHALL highlight invalid fields

### Requirement: Create wizard allows back navigation
The create wizard SHALL allow users to navigate back to previous steps.

#### Scenario: Back navigation
- **WHEN** user is on step 2, 3, or 4
- **AND** user clicks "Back" button
- **THEN** the wizard SHALL navigate to previous step
- **AND** SHALL preserve entered data

### Requirement: Create wizard shows summary on final step
The create wizard SHALL display a summary of all entered information on the final step.

#### Scenario: Review summary
- **WHEN** user reaches step 4 (Review)
- **THEN** the wizard SHALL display:
  - All entered values organized by step
  - Clear labels for each field
  - Ability to click to edit specific sections

### Requirement: Create wizard handles submission
The create wizard SHALL handle form submission with loading state and success/error handling.

#### Scenario: Submit operation
- **WHEN** user clicks "Create" button on final step
- **AND** the wizard SHALL:
  - Show loading state with "Creating..." text
  - Disable the submit button
  - Send API request
  - On success: close wizard, show success toast, refresh data
  - On error: show error toast, re-enable submit button

### Requirement: Create wizard allows cancellation
The create wizard SHALL allow users to cancel the operation at any time.

#### Scenario: Cancel operation
- **WHEN** user clicks "Cancel" button or X (close) button
- **AND** user has entered some data
- **THEN** the wizard SHALL show confirmation dialog
- **AND** If confirmed, close wizard and discard changes
- **AND** If cancelled, return to wizard

### Requirement: Create wizard resets on open
The create wizard SHALL reset all form values when reopened.

#### Scenario: Reset on open
- **WHEN** create wizard is opened
- **AND** wizard was previously used
- **THEN** all form fields SHALL be reset to default values
- **AND** progress SHALL be reset to step 1

### Requirement: Create cluster follows unified pattern
The cluster creation wizard SHALL follow the 4-step pattern.

#### Scenario: Cluster create wizard
- **WHEN** user clicks "Add Cluster" button
- **THEN** the 4-step wizard SHALL open
- **AND** Step 1: Name, Environment, Agent URL, Description
- **AND** Step 2: (Reserved for future: advanced config)
- **AND** Step 3: (Reserved for future: tags/labels)
- **AND** Step 4: Review and Confirm

### Requirement: Create job follows unified pattern
The job creation wizard SHALL follow the 4-step pattern (existing implementation).

#### Scenario: Job create wizard
- **WHEN** user clicks "Create Job" button
- **THEN** the 4-step wizard SHALL open (current implementation)
- **AND** Step 1: Select job type and job key
- **AND** Step 2: Configure parameters
- **AND** Step 3: Schedule configuration
- **AND** Step 4: Options and Review