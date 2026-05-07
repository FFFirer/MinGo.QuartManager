## MODIFIED Requirements

### Requirement: Template-based job creation
The system previously supported three template modes (Blank, Templates from manifest, Copy from existing) inside a SlidePanel.

**New behavior**: Template selection is replaced by:
- **New job**: Navigate to the full-page Create Job form (empty form, user fills in)
- **Copy from existing**: Click "Copy" action on a job row → navigate to `/schedulers/{name}/jobs/create?copyFrom=GROUP.name`

**Reason**: Unified into a single full-page form with URL-parameter-based copy flow. Removes the confusing multi-mode template selector from the old SlidePanel.

## REMOVED Requirements

### Requirement: Template selector in SlidePanel
**Reason**: Replaced by dedicated full-page form with URL-parameter copy support
**Migration**: Use `/schedulers/{name}/jobs/create` for new jobs, or click "Copy" on a job row and modify parameters
