# Job Create Form Schedule

## MODIFIED Requirements

### Requirement: Successful submission
The form SHALL submit a valid `CreateJobRequest` to the API.

**MODIFIED**: The previous response handling (only 200 success / generic error) is extended to handle the declarative creation response codes.

#### Scenario: Successful submission (200)
- **WHEN** user fills all required fields and clicks "Create Job"
- **THEN** system sends POST to `/api/schedulers/{name}/jobs` with assembled request body
- **AND** `jobKey` is composed as `"{group}.{name}"`
- **AND** on success (200), shows toast "Job created successfully!"
- **AND** navigates back to Jobs list

#### Scenario: Agent error on submission (502)
- **WHEN** Agent returns an error during job creation
- **THEN** system shows error toast with the Agent error message
- **AND** stays on the Create Job page

#### Scenario: Duplicate declaration (409)
- **WHEN** API returns HTTP 409 with message "Job已存在"
- **THEN** system shows warning toast "Job已存在，无需重复创建"
- **AND** stays on the Create Job page
