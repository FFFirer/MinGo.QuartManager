## ADDED Requirements

### Requirement: DataTable displays data in a tabular format
The DataTable component SHALL render data in a table with headers and rows.

#### Scenario: Empty data
- **WHEN** the DataTable component receives an empty data array
- **THEN** it SHALL render the table headers
- **AND** it SHALL render an empty state message if provided

#### Scenario: With data
- **WHEN** the DataTable component receives a non-empty data array
- **THEN** it SHALL render one row per data item
- **AND** each cell SHALL display the value of the corresponding data field

### Requirement: DataTable defines columns via column configuration
The DataTable component SHALL accept a columns prop to define the table structure.

#### Scenario: Column definition
- **WHEN** the DataTable component receives a columns array
- **THEN** it SHALL render a header cell for each column configuration
- **AND** the header text SHALL come from the column's header property
- **AND** the data for each cell SHALL be extracted using the column's accessor

#### Scenario: Column accessor as string
- **WHEN** a column's accessor is a string matching a data field name
- **THEN** the cell SHALL display the value of that field from the data item

#### Scenario: Column accessor as function
- **WHEN** a column's accessor is a function
- **THEN** the function SHALL be called with the data item
- **AND** the return value SHALL be displayed in the cell

#### Scenario: Column formatting
- **WHEN** a column has a format function
- **THEN** the raw cell value (from accessor) SHALL be passed to the format function
- **AND** the format function's return value SHALL be rendered in the cell

#### Scenario: Column width
- **WHEN** a column has a width property (string or number)
- **THEN** the column SHALL be rendered with the specified width

#### Scenario: Column alignment
- **WHEN** a column has an align property (left, center, right)
- **THEN** the cell contents SHALL be aligned accordingly

### Requirement: DataTable supports row selection
The DataTable component SHALL support highlighting a row on click or hover.

#### Scenario: Hover highlight
- **WHEN** the user hovers over a table row
- **THEN** the row SHALL display a hover background color

#### Scenario: Click selection (if enabled)
- **WHEN** the DataTable component receives an onRowClick prop
- **AND** the user clicks on a table row
- **THEN** the onRowClick function SHALL be called with the corresponding data item

### Requirement: DataTable supports loading and empty states
The DataTable component SHALL show appropriate states when loading or when there is no data.

#### Scenario: Loading state
- **WHEN** the DataTable component receives a loading prop set to true
- **THEN** it SHALL render a loading indicator instead of the table data

#### Scenario: Empty state with custom message
- **WHEN** the data array is empty
- **AND** an emptyMessage prop is provided
- **THEN** it SHALL render the emptyMessage instead of the table body

#### Scenario: Empty state with default message
- **WHEN** the data array is empty
- **AND** no emptyMessage prop is provided
- **THEN** it SHALL render a default empty state message (e.g., "No data available")

### Requirement: DataTable has consistent styling
The DataTable component SHALL use consistent styling for headers, cells, and borders.

#### Scenario: Header styling
- **WHEN** the DataTable renders column headers
- **THEN** they SHALL have a distinct background (e.g., bg-slate-700) and bold text

#### Scenario: Cell styling
- **WHEN** the DataTable renders data cells
- **THEN** they SHALL have consistent padding and text alignment

#### Scenario: Border styling
- **WHEN** the DataTable renders borders
- **THEN** they SHALL use a consistent border color (e.g., border-slate-700)

#### Scenario: Striped rows (optional)
- **WHEN** the DataTable is configured for striped rows
- **THEN** alternating rows SHALL have a subtle background variation (e.g., bg-slate-800/50)

### Requirement: DataTable supports optional features
The DataTable component MAY support additional features like sorting, but these are not required for the basic implementation.

#### Scenario: Sorting not required
- **WHEN** the DataTable component is used without sorting configuration
- **THEN** it SHALL still function correctly displaying data in the provided order