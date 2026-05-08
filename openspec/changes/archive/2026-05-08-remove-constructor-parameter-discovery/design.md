## Context

`JobDiscoveryService.DiscoverParameters(Type jobType)` has two discovery strategies in its history:

1. **Property-based** (active): Reflects properties with `[JobParameter]` / `[JobPayload]` attributes
2. **Constructor-parameter-based** (disabled): Was commented out previously — reflected constructor parameters, treating DI services as potential job parameters

The commented-out code remains in the source file, creating maintenance confusion. The principle going forward is: **only explicitly annotated properties are discovered** — no implicit inference from constructor signatures.

## Goals / Non-Goals

**Goals:**
- Remove dead commented-out code
- Add a self-documenting guard comment at the top of `DiscoverParameters()` to make the explicit-only principle clear
- Zero behavioral change

**Non-Goals:**
- No changes to the active discovery logic
- No changes to `[JobParameter]` or `[JobPayload]` attributes
- No new parameter discovery mechanisms

## Decisions

| Decision | Rationale |
|----------|-----------|
| Delete rather than keep commented | Dead code rots — it's not compiled, not tested, and creates ambiguity about intent |
| Single guard comment at method entry | More visible than inline comments around each loop — one reader sees the principle immediately |

## Risks / Trade-offs

- **[None]** — commented-out code deletion carries zero execution risk. The guard comment is a `//` comment with no runtime impact.
