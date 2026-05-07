## ADDED Requirements

### Requirement: JobType display component
The system SHALL provide a UI component that displays job type full names with assembly tag, right-ellipsis truncation, tooltip, and copy functionality.

#### Data model
The component SHALL accept structured `JobTypeQualifiedName` data:

```typescript
interface JobTypeQualifiedName {
  fullName: string;       // CLR Type.FullName, e.g. "Sample.Jobs.EchoJob"
  assembly: string;       // Assembly simple name, e.g. "Sample.Jobs"
  version?: string;
  culture?: string;
  publicKeyToken?: string;
}
```

#### Component layout
The component SHALL use a flex layout occupying full container width:

```
┌────────────────────────────────────────────────────┐
│ [assembly tag]        typename (right-ellipsis) [📋] │
└────────────────────────────────────────────────────┘
```

#### Scenario: Assembly displayed as tag
- **WHEN** a `JobTypeQualifiedName` with `assembly` is rendered
- **THEN** the assembly SHALL be displayed as a dark-background tag on the left
- **AND** the tag SHALL use `bg-slate-700 text-slate-300 text-xs px-2 py-1 rounded` styling
- **AND** the tag SHALL NOT be truncated (always fully visible)
- **WHEN** no `assembly` is present
- **THEN** no tag SHALL be rendered (typename starts from left)

#### Scenario: TypeName with right-ellipsis
- **WHEN** the `fullName` is rendered
- **THEN** it SHALL occupy the remaining space (`flex-1 min-w-0`)
- **AND** use standard `text-overflow: ellipsis` right-truncation
- **AND** the internal structure SHALL be: `namespace.` + `className`
  - `namespace` participates in ellipsis truncation
  - `className` (last segment after last `.`) is always fully visible
- **Example**: `Sample.Jobs.EchoJob` → `Sample.J...EchoJob` when space is limited

#### Scenario: Hover tooltip
- **WHEN** user hovers over the job type display
- **THEN** a tooltip SHALL appear showing the complete `"fullName, assembly"` composed string (e.g., `Sample.Jobs.EchoJob, Sample.Jobs`)

#### Scenario: Copy to clipboard
- **WHEN** user clicks the copy button next to the job type
- **THEN** the composed `"fullName, assembly"` string SHALL be copied to clipboard
- **AND** a brief "Copied" feedback SHALL be shown
- **AND** the copy button is optional via `showCopy` prop

#### Scenario: Table column display (compact)
- **WHEN** the component is rendered inside a DataTable cell with fixed column width
- **THEN** the tag SHALL remain fully visible (not truncated)
- **AND** the typename SHALL ellipsis appropriately
- **AND** the copy button MAY be hidden via `showCopy={false}` to save space
