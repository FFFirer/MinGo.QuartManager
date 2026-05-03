## ADDED Requirements

### Requirement: DataTable supports column sorting via header click
The DataTable component SHALL support sorting table data by clicking column headers, using TanStack Table's sort model internally.

#### Scenario: Click header to sort ascending
- **WHEN** the user clicks a column header that has sorting enabled
- **THEN** the column SHALL be sorted in ascending order
- **AND** the header SHALL display a visual indicator (e.g., arrow icon) showing ascending sort

#### Scenario: Click again to sort descending
- **WHEN** the user clicks the same column header again while it is in ascending sort
- **THEN** the column SHALL be sorted in descending order
- **AND** the header SHALL display a visual indicator showing descending sort

#### Scenario: Click again to remove sort
- **WHEN** the user clicks the same column header again while it is in descending sort
- **THEN** the sort SHALL be cleared
- **AND** the column SHALL return to its original order

#### Scenario: Sortable column configuration
- **WHEN** a column's `sortable` prop is set to `true`
- **THEN** clicking its header SHALL toggle sorting on that column
- **WHEN** a column's `sortable` prop is set to `false` or not provided
- **THEN** clicking its header SHALL NOT trigger sorting

#### Scenario: Multi-column sort not required
- **WHEN** a column is being sorted
- **THEN** clicking a different column header SHALL replace the current sort with the new column's sort
- **AND** multi-column sorting is not required for the basic implementation

### Requirement: DataTable displays sort indicators in headers
The DataTable component SHALL render visual indicators in column headers when sorting is active.

#### Scenario: Default unsorted state
- **WHEN** a column is not sorted
- **THEN** the header cell MAY display a muted sort icon to indicate sortability

#### Scenario: Ascending indicator
- **WHEN** a column is sorted in ascending order
- **THEN** the header cell SHALL display an ascending arrow indicator (e.g., ↑)

#### Scenario: Descending indicator
- **WHEN** a column is sorted in descending order
- **THEN** the header cell SHALL display a descending arrow indicator (e.g., ↓)

### Requirement: DataTable supports controlled sorting
The DataTable component SHALL support external sort state via props for server-side sorting scenarios.

#### Scenario: Controlled sort state
- **WHEN** `sortBy` and `sortOrder` props are provided
- **THEN** the table SHALL display the column indicated by `sortBy` sorted according to `sortOrder`
- **AND** clicking a header SHALL call the `onSortChange` callback instead of changing internal state

#### Scenario: Uncontrolled sort state
- **WHEN** no `sortBy`/`sortOrder` props are provided
- **THEN** the table SHALL manage sort state internally
