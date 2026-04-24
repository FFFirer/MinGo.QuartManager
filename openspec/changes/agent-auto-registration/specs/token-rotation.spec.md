## ADDED Requirements

### Requirement: Token rotation on expiry or compromise
Description: The agent should rotate its authentication token in a secure manner when it expires or when a compromise is suspected.

#### Scenario: Token rotation on expiry
- **WHEN** the current token nears expiry
- **THEN** the agent requests a new token from the Platform and updates stored credentials

### Requirement: Secure token storage and rotation audit
Description: All tokens must be stored securely and rotation events should emit auditable traces for compliance and troubleshooting.

#### Scenario: Rotation audit trail
- **WHEN** a token rotation occurs
- **THEN** an auditable event is emitted including timestamp, old token reference (hashed), and new token reference (hashed)
