## ADDED Requirements

### Requirement: Type-aware parameter rendering
The system SHALL render Job parameters according to their runtime value type. Boolean values SHALL display as icon badges (✓/✗). Numeric values SHALL be right-aligned with monospaced digits. Date/time strings SHALL format with `toLocaleString()` and show ISO timestamp on hover. Object and array values SHALL display as collapsible JSON trees, default collapsed. String values SHALL display as plain text with a copy button.

#### Scenario: Bool parameter renders as icon badge
- **WHEN** a parameter has value `true` or `false`
- **THEN** it SHALL render as a colored icon badge: green checkmark for `true`, gray x-mark for `false`

#### Scenario: Number parameter renders with monospaced digits
- **WHEN** a parameter value is a JavaScript number
- **THEN** it SHALL be rendered right-aligned with `tabular-nums` font variant

#### Scenario: DateTime string renders with human-readable format
- **WHEN** a parameter value is a string that parses as a valid ISO date
- **THEN** it SHALL display `toLocaleString()` format and show the raw ISO string in a tooltip on hover

#### Scenario: Object parameter renders as collapsible JSON tree
- **WHEN** a parameter value is a plain object or array
- **THEN** it SHALL render as a collapsible tree view, default collapsed, with expand/collapse toggle

#### Scenario: Long text parameter is truncated
- **WHEN** a string parameter value exceeds 80 characters
- **THEN** it SHALL be truncated with "..." and a "Show more" / "Show less" toggle

### Requirement: Manifest metadata integration
The system SHOULD attempt to match runtime parameters against manifest definitions by parameter name. When a match is found, the display SHOULD use the `label` field as the display name, show a "required" indicator for required parameters, show the default value as a subdued hint when current value equals default, and show a small type badge (e.g., "int", "bool") next to the label.

#### Scenario: Label from manifest replaces raw key
- **WHEN** a parameter matches a manifest definition that has a `label` field
- **THEN** the `label` SHALL be displayed instead of the raw parameter name

#### Scenario: Required parameter shows indicator
- **WHEN** a parameter matches a manifest definition with `required: true`
- **THEN** a red asterisk SHALL appear next to the parameter label

#### Scenario: Missing manifest falls back to raw key
- **WHEN** manifest data is not loaded or the parameter name is not in the manifest
- **THEN** the raw parameter key SHALL be displayed and type SHALL be inferred from runtime value

### Requirement: Parameter search and filter
The system SHALL provide a search input above the parameter list. The search SHALL be case-insensitive and match against parameter name, label, and string representation of the value. Results SHALL update in real-time as the user types. When no parameters match, a "No matching parameters" message SHALL display.

#### Scenario: Search filters parameters in real-time
- **WHEN** user types in the search input
- **THEN** the parameter list SHALL filter to show only matching parameters

#### Scenario: Empty search shows all parameters
- **WHEN** the search input is empty
- **THEN** all parameters SHALL be displayed

#### Scenario: No match shows empty state
- **WHEN** no parameters match the search query
- **THEN** a "No matching parameters" message SHALL be displayed

### Requirement: Copy parameter value
Each parameter value SHALL have a copy button. Clicking the button SHALL copy the parameter's string representation to the clipboard. A brief visual feedback (tooltip or color flash) SHALL indicate successful copy.

#### Scenario: Copy button copies value to clipboard
- **WHEN** user clicks the copy button on a parameter
- **THEN** the parameter value SHALL be copied to clipboard and a "Copied!" tooltip SHALL appear briefly

### Requirement: Defensive deserialization
The component SHALL gracefully handle the case where `params` is received as a serialized JSON string. If `typeof params === 'string'`, it SHALL attempt `JSON.parse()` and use the result. If parsing fails, it SHALL display an empty state and log a warning to the console.

#### Scenario: Params is a JSON string
- **WHEN** `params` is a string that can be parsed as a JSON object
- **THEN** the component SHALL parse it and render the resulting object

#### Scenario: Params parsing fails
- **WHEN** `params` is a string that is not valid JSON
- **THEN** the component SHALL display an empty state and log a console.warning
