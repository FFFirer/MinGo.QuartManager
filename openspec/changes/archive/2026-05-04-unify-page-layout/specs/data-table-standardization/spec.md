## ADDED Requirements

### Requirement: DataTable column header accepts ReactNode
The DataTable `Column.header` type SHALL accept `string | ReactNode` to support custom header content (e.g., checkbox for batch selection).

#### Scenario: Header as string
- **WHEN** a column's header is a plain string
- **THEN** it SHALL render as text in the header cell

#### Scenario: Header as ReactNode
- **WHEN** a column's header is a ReactNode (e.g., checkbox input)
- **THEN** it SHALL render the ReactNode directly in the header cell

### Requirement: All list pages use DataTable for tabular data
Pages that display tabular data SHALL use the DataTable component instead of native `<table>` or custom CSS grid implementations.

#### Scenario: AgentsPage uses DataTable
- **WHEN** AgentsPage renders agent list
- **THEN** it SHALL use DataTable component with defined columns

#### Scenario: SchedulersPage uses DataTable
- **WHEN** SchedulersPage renders scheduler list
- **THEN** it SHALL use DataTable component instead of native `<table>`

#### Scenario: Detail page sub-tables use DataTable
- **WHEN** AgentDetailPage renders associated schedulers table
- **THEN** it SHALL use DataTable component
- **WHEN** SchedulerDetailPage renders associated agents table
- **THEN** it SHALL use DataTable component

### Requirement: Page wrapper uses consistent className
All page components SHALL use `<div className="p-6">` as the outermost wrapper, delegating global background and text styling to App.tsx's AppLayout.

#### Scenario: AgentsPage wrapper
- **WHEN** AgentsPage renders
- **THEN** the outermost element SHALL be `<div className="p-6">`

#### Scenario: AgentDetailPage wrapper
- **WHEN** AgentDetailPage renders (including loading and error states)
- **THEN** the outermost element SHALL be `<div className="p-6">`

### Requirement: PageHeader is used correctly in AgentDetailPage
AgentDetailPage SHALL use PageHeader's `backPath` prop for back navigation and SHALL NOT manually render `<h1>` titles or `<Link>` back buttons.

#### Scenario: Back navigation via PageHeader
- **WHEN** AgentDetailPage renders
- **THEN** PageHeader SHALL receive `backPath="/agents"` prop
- **AND** there SHALL be no manual `<Link to="/agents">← Back to Agents</Link>` element outside PageHeader

#### Scenario: Title via PageHeader only
- **WHEN** AgentDetailPage renders
- **THEN** the agent name title SHALL be rendered solely by PageHeader's `title` prop
- **AND** there SHALL be no separate `<h1>` element rendering the agent name
