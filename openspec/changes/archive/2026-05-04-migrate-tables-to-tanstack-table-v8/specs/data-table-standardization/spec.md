## ADDED Requirements

### Requirement: DataTable uses native table elements for rendering
The DataTable component SHALL use native `<table>`, `<thead>`, `<tbody>`, `<tr>`, `<th>`, `<td>` elements instead of flex-wrap or CSS grid for layout.

#### Scenario: Table structure
- **WHEN** the DataTable renders
- **THEN** the outermost container SHALL be a `<table>` element
- **AND** column headers SHALL be rendered in `<thead>` with `<tr>` and `<th>` elements
- **AND** data rows SHALL be rendered in `<tbody>` with `<tr>` and `<td>` elements

#### Scenario: Column alignment in native table
- **WHEN** a column has an `align` property set to `left`, `center`, or `right`
- **THEN** the corresponding `<th>` or `<td>` elements SHALL have the appropriate `text-align` class

### Requirement: List pages use PaginationBar for pagination controls
Pages with paginated table data SHALL use the PaginationBar component instead of manually rendering pagination buttons.

#### Scenario: AgentsPage pagination
- **WHEN** AgentsPage renders pagination controls
- **THEN** it SHALL use PaginationBar component
- **AND** the page SHALL pass current page, total pages, page size, and total items as props

#### Scenario: JobsPage pagination
- **WHEN** JobsPage renders pagination controls
- **THEN** it SHALL use PaginationBar component
- **AND** the page SHALL pass current page, total pages, page size, and total items as props

## MODIFIED Requirements

### Requirement: DataTable column header accepts ReactNode
The DataTable `Column.header` type SHALL accept `string | ReactNode` to support custom header content (e.g., checkbox for batch selection). The underlying implementation SHALL use TanStack Table's column definition model internally.

#### Scenario: Header as string
- **WHEN** a column's header is a plain string
- **THEN** it SHALL render as text in the header cell

#### Scenario: Header as ReactNode
- **WHEN** a column's header is a ReactNode (e.g., checkbox input)
- **THEN** it SHALL render the ReactNode directly in the header cell

### Requirement: All list pages use DataTable for tabular data
Pages that display tabular data SHALL use the DataTable component instead of native `<table>` or custom CSS grid implementations. The DataTable SHALL use TanStack Table v8 as its internal data management engine.

#### Scenario: AgentsPage uses DataTable
- **WHEN** AgentsPage renders agent list
- **THEN** it SHALL use DataTable component with defined columns
- **AND** the DataTable SHALL use TanStack Table internally

#### Scenario: SchedulersPage uses DataTable
- **WHEN** SchedulersPage renders scheduler list
- **THEN** it SHALL use DataTable component instead of native `<table>`

#### Scenario: Detail page sub-tables use DataTable
- **WHEN** AgentDetailPage renders associated schedulers table
- **THEN** it SHALL use DataTable component
- **WHEN** SchedulerDetailPage renders associated agents table
- **THEN** it SHALL use DataTable component
