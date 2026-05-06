# Analytics Event Taxonomy

Every event dispatched through `Analytics.Track(eventName, payload)` is registered here. The MVP backend is `NullAnalyticsSink` (no-op); the commercial-phase backend is decided at the end of Alpha. Adding a new event = a row here + the call site.

## Conventions

- **eventName**: `lower.snake.case` with dot-separated subjects. Examples: `session.start`, `command.rejected`, `phase.unlocked`.
- **payload**: `IReadOnlyDictionary<string, object>` with stable key names. Document each key's type and purpose. Personally identifying information is forbidden — use opaque session IDs only.
- **added-in-phase**: phase number where the event ships (Phase 0–10).

## Events

| eventName | payload | added-in-phase | notes |
|---|---|---|---|

_(no events yet — Phase 0 ships the `Analytics.Track` facade with the `NullAnalyticsSink` backend; first real events arrive in Phase 1+ when commands and tick metrics start firing.)_

## Privacy and consent

When the commercial-phase backend is wired (per Tech Roadmap Appendix F — Commercialization Checklist):
- Opt-in UI required at first launch.
- No PII; opaque session ID only.
- All events documented here before they ship.
- Privacy policy page links to this taxonomy.
