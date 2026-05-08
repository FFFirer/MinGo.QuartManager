## MODIFIED Requirements

### Requirement: User can enter Job Name
The form SHALL provide a required text input for the job name. The job name SHALL only contain alphanumeric characters, hyphens (`-`), and underscores (`_`).

#### Scenario: Job name is required
- **WHEN** user submits the form without entering a job name
- **THEN** system shows validation error "Job name is required"

#### Scenario: Job name contains invalid characters
- **WHEN** user enters a job name containing characters other than letters, digits, hyphens, or underscores
- **THEN** system shows validation error "Job name只能包含字母、数字、-和_"
- **AND** submission is blocked

### Requirement: User can select or enter Job Group
The form SHALL provide a Group field positioned to the right of the Job Name field, allowing selecting from existing groups or entering a custom group name.

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

### Requirement: User can enter Job Identity (Name and Group)
The form SHALL display Name first (left column) and Group second (right column) in the Job Identity section.

#### Scenario: Name field appears before Group field
- **WHEN** user is on the Create Job page
- **THEN** the Job Identity section SHALL show Name on the left column and Group on the right column
- **AND** the Full Job Key preview shows as `{group}.{name}`

### Requirement: Required parameters are validated
The form SHALL validate that all required parameters (marked with `required: true` in manifest) have values before submission. Parameters with default values SHALL be pre-filled and not trigger validation errors when left unchanged.

#### Scenario: Required parameter with default value passes validation when unchanged
- **WHEN** a required parameter has a `default` value defined in the manifest
- **AND** user selects the job type (which pre-fills the default)
- **AND** user does not modify the parameter value
- **THEN** system pre-fills `params[name] = default` automatically
- **AND** validation passes without requiring user interaction

#### Scenario: Required parameter missing shows error
- **WHEN** user submits the form with a required parameter that has no default value and is left empty
- **THEN** system shows inline error below the empty required field
- **AND** submission is blocked

#### Scenario: Required parameter marked visually
- **WHEN** a parameter is required
- **THEN** its label has a red asterisk `*` suffix
