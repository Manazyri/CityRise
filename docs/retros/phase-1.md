# Phase 1 retrospective — Core framework

**Estimate (Tech Roadmap §6.3):** 2–3 weeks focused work
**Actual:** ~10 hours of active AI-paired implementation, spread over ~21 hours wall-clock (2026-05-06 → 2026-05-07)
**Tag:** `v0.1.0-framework`
**Commits:** 11 (`d16a818` → `c84954c`)

## Acceptance criteria (Tech Roadmap §6.3)

- [x] TickScheduler at sim 1 Hz / growth 0.1 Hz / monthly budget; speed multiplier; sim time authoritative
- [x] CommandBus apply-only with `Result<Unit>`; undo/redo bounded to 50; OnApplied / OnRejected event surface
- [x] EventBus queued pub/sub; flush at tick boundary; events as `readonly struct`
- [x] Persistence skeleton — ISaveable, SaveManifest, JsonSaveBackend, schema-version header, MigrationRegistry, atomic writes
- [x] Cinemachine RTS camera — pan, zoom, edge-pan
- [x] Time-control UI in UI Toolkit — pause / 1× / 2× / 3× with Space/1/2/3 hotkeys
- [x] Debug console — backtick toggle, attribute-registered commands, Tab autocomplete, Up/Down history
- [x] UI Toolkit shell — HUD root, top bar, bottom toolbar slot, right panel slot
- [x] TickMetrics — Profiler markers + 3-tick over-budget warnings
- [x] Replay recorder scaffolding (recorder only; player follows in Phase 2+)
- [x] Save/load round-trips camera + time speed (verified by `Phase1RoundTripTests`)
- [x] Tagged `v0.1.0-framework`

## What went well

- **Pure-logic primitives shipped first** (TickScheduler / CommandBus / EventBus / TickMetrics on Day 1, Persistence on Day 2). Each landed with comprehensive unit tests; later UI work could lean on stable foundations.
- **CI caught real bugs immediately.** The EventBus mid-flush republish bug and the SaveBlob integer-read type mismatch both fired on the first push, exactly what tests are meant for.
- **Memory system paid for itself.** Lessons from Phase 0 ("write canonical YAML directly", "use boring Unity patterns") loaded automatically and shaped the right approach without re-derivation. P1.D4 onwards used direct YAML edits successfully on first try; Day 5's pain wasn't repeated.
- **CinemachineBrain + CinemachineCamera scene wiring landed first try** for the same reason — templated from a working sample in `Library/PackageCache`.
- **Layer hierarchy held.** No CLAUDE.md violations introduced. `InputContext` was carved into Core specifically to avoid a UI ↔ Debug cycle when the time-control panel needed to know about the console's input ownership.

## What to improve

- **Unity 6 + new Input System interactions are buggy.** TextField throws `ArgumentOutOfRangeException` in `DeleteSelection` on first character insert. UI Toolkit panel focus interferes with `Keyboard.current.wasPressedThisFrame` polling. Solution arrived after three architectural iterations (TextField → Label + key-polling → Label + `onTextInput` global event). The third works reliably; the path getting there cost ~1.5 hours of debugging. Lesson saved to `feedback_unity_input_layering.md` — when building a modal text-input UI in Unity 6, route characters through `Keyboard.current.onTextInput` and special keys through Update polling; don't rely on TextField or UI Toolkit focus.
- **UI Toolkit font cascade is non-obvious.** Elements added directly to `rootVisualElement` don't inherit `-unity-font` set on a subtree. The DebugConsole rendered glyph-less Labels until I moved it under `hud-root`. Memory updated.
- **The DebugConsole input went through three architectures before landing.** Too many reactive small fixes; would have benefited from a 5-minute pause to think through "what is the minimum set of input-routing assumptions that hold in Unity 6 + new Input System + UI Toolkit + click-to-focus" before writing. Documented in the YAML/conventions memories so future Unity-bound input doesn't repeat the loop.
- **MemoryPack still deferred.** Carries from Phase 0 again. Persistence works on JSON only; binary backend remains a Phase 2 task. Acceptable for Phase 1 (round-trip works), but the original Tech Roadmap §6.3 deliverable mentioned "MemoryPack binary container" — we shipped JSON-only.
- **Phase-1 save/load testing is not yet run end-to-end in play mode.** `Phase1RoundTripTests` covers the in-memory shape via the SaveService directly, but the full Bootstrap + scene + auto-registration loop hasn't been exercised by a manual play-mode round-trip. Phase 2 should include a "press Play, run dump_state, mutate state, run load_state, observe restoration" smoke test.

## Hours estimate vs. actual

| | Estimate | Actual |
|---|---|---|
| P1.D1 — TickScheduler / CommandBus / EventBus / TickMetrics | ~1 week human | ~1.5 hours active (incl. CI fix cycle) |
| P1.D2 — Persistence skeleton | ~3 days human | ~1.5 hours active (incl. CI fix cycle) |
| P1.D3 — ReplayRecorder + CommandBus.OnApplied | ~1 day human | ~1 hour active |
| P1.D4 — UI Toolkit HUD shell + scene wire | ~2 days human | ~1.5 hours active (incl. font-cascade discovery) |
| P1.D5 — Cinemachine RTS camera | ~1.5 days human | ~1 hour active |
| P1.D6 — Time-control UI + hotkeys | ~1 day human | ~1 hour active |
| P1.D7 — Debug console | ~2 days human | ~2.5 hours active (incl. 3 input-routing architectures) |
| P1.End — Save/load round-trip + tag | ~0.5 days human | ~0.5 hours active |
| **Total** | **2–3 weeks** | **~10 hours active** |

Same compression ratio as Phase 0 — boilerplate-heavy work (facades, tests, scene YAML) compresses well; novel discovery (Unity 6 input quirks, font cascade) doesn't. The gap will continue to narrow as later phases shift toward design and balancing decisions that don't compress as easily.

## Lessons captured to memory

- **`feedback_unity_conventions.md`** (carried from Phase 0; reinforced by P1.D4–D6 success): traditional braced namespaces and non-nullable `[SerializeField] = null!` defaults for Unity-bound types.
- **`feedback_unity_yaml_editing.md`** (carried from Phase 0; used productively in P1.D4, D5, D6, D7): template canonical YAML from `Library/PackageCache` samples. Avoid `script-execute` for MonoBehaviour wiring.
- **New for Phase 1 — UI Toolkit input architecture:**
  - Don't use `TextField` for text input in Unity 6 + new Input System (DeleteSelection bug)
  - Use `Keyboard.current.onTextInput` for character input (global, focus-independent, layout-aware)
  - Use Update polling for special keys (Enter / Escape / Backspace / Tab / Up / Down / global toggles)
  - Make sure UI elements that need text rendered are descendants of an element with `-unity-font` set
  - Wrap log/transcript outputs in `ScrollView` rather than fighting Label alignment + overflow

(Memory file consolidation deferred to Phase 2 entry — the existing two files plus the lessons listed above span the whole UI/Unity story.)

## Carry-forward to Phase 2

- **Install MemoryPack** (now genuinely needed for binary saves; current JSON path works for dev but produces non-compact files). Carries over from Phases 0 and 1.
- **Run a manual play-mode save/load smoke test** — open Boot, change tick speed and camera position, `dump_state`, mutate, `load_state`, verify restoration. Add the result to the Phase 2 retro.
- **Grid + WorldState** per Tech Roadmap §6.4 (1-week effort estimate). This is the substrate everything from Phase 3 onward sits on.
- **Add `WorldState` parameter to `ITickStep` and `ICommand` signatures** when WorldState lands — Phase 1 stubbed those interfaces parameter-less.
- **Phase 2 ALSO triggers** schema-version-1-to-2 migration of the existing TimeControlSaveState if its shape changes — good first real exercise of the migration framework.
