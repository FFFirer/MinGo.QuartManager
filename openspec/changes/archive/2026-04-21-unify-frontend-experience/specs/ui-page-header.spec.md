## ADDED Requirements

### Requirement: PageHeader displays title and optional subtitle
The PageHeader component SHALL display a main title and an optional subtitle.

#### Scenario: Title only
- **WHEN** the PageHeader component receives only a title prop
- **THEN** it SHALL render the title prominently
- **AND** it SHALL NOT render a subtitle

#### Scenario: Title with subtitle
- **WHEN** the PageHeader component receives both title and subtitle props
- **THEN** it SHALL render both the title and subtitle
- **AND** the subtitle SHALL be visually subordinate to the title

### Requirement: PageHeader supports breadcrumb navigation
The PageHeader component SHALL support displaying a breadcrumb trail.

#### Scenario: No breadcrumbs
- **WHEN** the PageHeader component receives no breadcrumbs prop or an empty array
- **THEN** it SHALL NOT render any breadcrumb navigation

#### Scenario: With breadcrumbs
- **WHEN** the PageHeader component receives a breadcrumbs array with items
- **THEN** it SHALL render each breadcrumb item as a navigable link except the last item
- **AND** the last breadcrumb item SHALL be marked as active and non-navigable
- **AND** breadcrumb items SHALL be separated by a divider character (e.g., "/" or ">")

#### Scenario: Breadcrumb navigation
- **WHEN** a user clicks on a non-active breadcrumb link
- **THEN** the browser SHALL navigate to the path specified in that breadcrumb's path prop

### Requirement: PageHeader supports back navigation
The PageHeader component SHALL support a back navigation button.

#### Scenario: No back path
- **WHEN** the PageHeader component receives no backPath prop
- **THEN** it SHALL NOT render a back navigation button

#### Scenario: With back path
- **WHEN** the PageHeader component receives a backPath prop
- **THEN** it SHALL render a back navigation button (typically with an arrow icon)
- **AND** clicking the button SHALL navigate to the specified backPath

### Requirement: PageHeader displays status indicator
The PageHeader component SHALL support displaying a status indicator.

#### Scenario: No status
- **WHEN** the PageHeader component receives no status prop
- **THEN** it SHALL NOT render a status indicator

#### Scenario: With status
- **WHEN** the PageHeader component receives a status prop
- **THEN** it SHALL render the status using the StatusBadge component
- **AND** the status SHALL be positioned appropriately in the header layout

### Requirement: PageHeader supports action buttons
The PageHeader component SHALL support displaying action buttons.

#### Scenario: No actions
- **WHEN** the PageHeader component receives no actions prop
- **THEN** it SHALL NOT render any action buttons in the header

#### Scenario: With actions
- **WHEN** the PageHeader component receives an actions prop containing JSX elements
- **THEN** it SHALL render those elements in the header's action area
- **AND** the actions SHALL be aligned appropriately (typically right-aligned)

### Requirement: PageHeader supports custom children
The PageHeader component SHALL support rendering custom children elements.

#### Scenario: No children
- **WHEN** the PageHeader component receives no children prop
- **THEN** it SHALL render only the standard header elements (title, breadcrumbs, etc.)

#### Scenario: With children
- **WHEN** the PageHeader component receives a children prop
- **THEN** it SHALL render those children in a designated area of the header
- **AND** the children SHALL appear after the standard header elements