## Why

Job parameter discovery must be **explicit** — only properties annotated with `[JobParameter]` or `[JobPayload]` should be discovered. Constructor parameter sniffing was already disabled (commented out) but dead code remains, creating confusion about intent. This change removes the dead code and enforces the principle at the code level.

## What Changes

- Delete the commented-out constructor parameter discovery code block (40 lines) in `JobDiscoveryService.DiscoverParameters()`
- Add an explicit guard comment in `DiscoverParameters()` stating that only explicitly annotated properties are discovered — no implicit/unannotated inference
- No behavioral changes — the active code path (property-based discovery via `[JobParameter]` / `[JobPayload]`) is untouched

## Capabilities

### New Capabilities
<!-- No new capabilities — this is a code health cleanup -->

### Modified Capabilities
<!-- No spec-level behavior changes — this is internal cleanup only -->

## Impact

- **File**: `src/MinGo.Qap.Agent/Services/JobDiscoveryService.cs`
- **Nature**: Dead code removal + documentation guard
- **Risk**: None — commented-out code is not executed, behavior is unchanged
