# CityRise — Handoff to Claude Code

This folder contains everything Claude Code needs to begin Phase 0 of the CityRise Unity project. The plan and architecture were developed across several review passes in Cowork; this package is the bridge into hands-on implementation.

## What's in this folder

```
CityRise_Handoff/
├── HANDOFF.md                       (this file)
├── CLAUDE.md                        (drop into project root; AI context)
├── PHASE_0_TASKS.md                 (sequenced Phase 0 work)
└── docs/
    ├── perf-budget.md               (system budgets — drop into /docs/)
    └── adr/
        ├── 0001-engine-choice.md
        ├── 0002-terrain-custom-chunked.md
        ├── 0003-roads-unity-splines.md
        ├── 0004-hybrid-simulation.md
        ├── 0005-apply-only-commands.md
        ├── 0006-data-driven-content.md
        ├── 0007-worldstate-view-split.md
        ├── 0008-presentation-mirror.md
        └── 0009-typed-entity-ids.md
```

The canonical `CityRise_GDD.docx` and `CityRise_Tech_Roadmap.docx` (v0.4) are in the parent `outputs/` folder. They are the source of truth for design and architecture; copy them to the new project's `/docs/` folder during Phase 0 (or keep them in a separate docs repo — your call).

## Manual steps you must do BEFORE Claude Code can take over

These can't be delegated; do them in this order:

1. **Install Rider** if you don't already have it. Sign in for the free non-commercial license if eligible. https://www.jetbrains.com/rider/
2. **Install Unity Hub**, then install **Unity 6 LTS** through it. https://unity.com/download
3. **Create a new Unity project** from the **3D (URP)** template. Name it `CityRise`. Pick a path on your machine you'll remember — say `~/Projects/CityRise` (Linux/Mac) or `D:\Projects\CityRise` (Windows). Open it once in Unity to confirm it builds and the URP renderer is active.
4. **Install the Unity MCP plugin** (CoplayDev/unity-mcp). In Unity, open `Window → Package Manager → + → Add package from git URL`, paste:
   ```
   https://github.com/CoplayDev/unity-plugin.git#beta
   ```
   Follow CoplayDev's setup instructions to start the MCP server inside Unity.
5. **Install Claude Code** in your terminal. https://claude.com/code (or `npm install -g @anthropic-ai/claude-code` if available — verify the current install instructions).
6. **Install the Claude Code Rider plugin** via JetBrains Marketplace inside Rider (`File → Settings → Plugins → Marketplace → search "Claude Code"`).
7. **Authenticate Claude Code** with your Anthropic account (`claude` in terminal will walk you through it).
8. **Create a private GitHub repository** named `CityRise`. You can do this in the GitHub UI or via `gh repo create CityRise --private --source=. --remote=origin` from the project folder once Claude Code starts.
9. **Open a terminal in your Unity project root**. Run `claude`. This starts Claude Code in your project context.
10. **Add the Unity MCP to Claude Code**: `claude mcp add` and follow prompts to point at the running CoplayDev MCP server.
11. **Optional but recommended**: Install the `unity-claude-skills` plugin via `/plugins` in Claude Code for grounded Unity 6 LTS API knowledge.

## What you say to Claude Code on first session

Once everything above is done, drop this verbatim into Claude Code as your first message:

> Read these files and acknowledge:
> 1. `HANDOFF.md` (in the same folder I copied here from)
> 2. `CLAUDE.md` (project root)
> 3. `PHASE_0_TASKS.md`
> 4. The nine ADRs in `docs/adr/`
> 5. The GDD and Tech Roadmap in `docs/` (use pandoc if needed for the docx)
>
> Then start Phase 0 from PHASE_0_TASKS.md. Tell me which tasks need my input vs which you can drive autonomously.

Claude Code will then take over Phase 0 execution.

## What Claude Code will do (high level)

- Read every doc in this handoff for full context
- Set up the Git repo, .gitignore, .gitattributes, GitHub Actions CI
- Create the layered asmdef structure under `Assets/_CityRise/Code/`
- Write all Core facades (GameConstants, Log, I18n, FeatureFlags, EntityId<T>, RNG, Result<T>, NotificationBus, IterateSorted, Accessibility stub, Analytics stub)
- Wire the `Bootstrap` GameObject in the Boot scene (via Unity MCP)
- Get CI green
- Tag `v0.0.1-setup` and merge to main

## What stays in Cowork

Use Cowork (this conversation, or new ones) for:
- Major design or doc revisions to the GDD or Tech Roadmap
- High-level "where are we in the plan" sessions
- Producing or revising visual artifacts (slides, polished docs)

Daily implementation work belongs in Claude Code.
