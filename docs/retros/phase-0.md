# Phase 0 retrospective — Setup, tech stack, Phase-0 facades

**Estimate (Tech Roadmap §6.2):** 1–2 weeks focused work
**Actual:** ~6 hours of active AI-paired implementation, spread over ~12 hours wall-clock (2026-05-05 → 2026-05-06)
**Tag:** `v0.0.1-setup`
**Commits:** 8 (`6b80354` → `db8377b`)

## Acceptance criteria (Tech Roadmap Appendix E)

- [x] Fresh clone builds in Unity 6 LTS without manual setup — proven by green CI
- [x] GitHub Actions completes a Windows build on main (green)
- [x] README, CLAUDE.md, CONTRIBUTING.md exist and are non-trivial (68 / 91 / 106 lines)
- [x] ADR folder has the seed ADRs — 9 of 9 (0001–0009), all `Status: Accepted`
- [x] Core facades compile and expose documented APIs (Log, I18n, FeatureFlags, EntityId\<T\>, Result\<T\>, NotificationBus, IterateSorted, Accessibility, Analytics)
- [x] Boot scene loads Main; Bootstrap wires services without errors
- [x] Tagged `v0.0.1-setup`

## What went well

- **Layered asmdef structure** held up cleanly through 5 days of additions. Downward dependencies are enforced at compile time; no leakage.
- **78 EditMode unit tests** all green on first authoring; no test-driven debugging needed.
- **CI green by end of Day 6** including Library cache key, LFS handling, and runner disk-space cleanup.
- **Unity-conventions lesson** captured early in Claude Code memory (file-scoped namespaces vs MonoBehaviour Inspector binding), so future MonoBehaviour authoring defaults to the boring shape.

## What to improve

- **Day 5 scene assembly was a rabbit hole.** MCP-driven `script-execute` calls created stub MonoScript references that broke Inspector field rendering. Lost ~2 hours fighting Unity's MonoScript table cache, which only fully refreshes on editor restart + ScriptAssemblies cache flush. Lesson: for Unity-bound types (MonoBehaviour / ScriptableObject), write canonical YAML directly with proper GUID refs and stay clear of MCP-driven scene mutation until the plugin handles MonoScript binding cleanly.
- **Unity license activation workflow was deprecated.** `game-ci/unity-request-activation-file@v2` no longer works; current flow is local Unity Hub activation. Should've checked the action's status before pushing the one-shot workflow.
- **GitHub Actions secrets needed `UNITY_EMAIL` + `UNITY_PASSWORD`** in addition to `UNITY_LICENSE`, which wasn't obvious from our handoff. Lost a CI cycle before discovering this.
- **Permissions on `unity-test-runner`**: workflow needs `checks: write` to post results, otherwise the check post-step fails after tests pass. One more CI cycle lost.
- **MemoryPack deferred to Phase 1** — task #11 was pre-planned as Phase 0 but the Roslyn analyzer plumbing isn't worth fighting before persistence work begins.

## Hours estimate vs. actual

| | Estimate | Actual |
|---|---|---|
| Days 1–4 (project skeleton, asmdefs, Core facades) | ~3 days human | ~1.5 hours active |
| Day 5 (Bootstrap + scenes) | ~1 day human | ~3 hours active (incl. scene-wiring rabbit hole) |
| Day 6 (CI) | ~1 day human | ~2 hours active (incl. 2 failed CI cycles) |
| Day 7 (docs polish + tag) | ~0.5 days human | ~30 min active |
| **Total** | **1–2 weeks** | **~6 hours active** |

Phase-0 estimate was based on solo human work; AI-paired implementation accelerates the boilerplate-heavy parts (facades, tests, commit messages) significantly. Expect the gap to narrow in later phases as work shifts toward design and balancing decisions that don't compress as easily.

## Carry-forward to Phase 1

- Install MemoryPack at Phase 1 start (task carries over from Day 2 #11).
- Build any new MonoBehaviour-bearing scenes via Unity UI rather than MCP-driven scene scripting.
- Add Phase 1 systems (TickScheduler, CommandBus, EventBus) per Tech Roadmap §6.3.
