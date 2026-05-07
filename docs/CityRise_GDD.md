**Game Design Document**

Working Title: CityRise

*A stylized low-poly 3D city-builder for PC*

Version 0.1 — Draft — April 2026

**1. Document Information**

|  |  |
|----|----|
| **Document** | Game Design Document (GDD) + Tools & Tech Stack |
| **Status** | Draft v0.1 |
| **Owner** | Mark (Lead / Programmer) |
| **Author** | Team |
| **Last updated** | April 2026 |
| **Target platform** | PC (Windows primary; Linux/Mac via Unity cross-compile) |
| **Target distribution** | itch.io during hobby phase; Steam if commercial |
| **Engine** | Unity (latest LTS) |
| **Language** | C# |
| **Team** | 2 people: 1 programmer (lead), 1 3D modeler |

**2. High Concept**

A stylized low-poly 3D city-builder for PC in which the player founds a settlement, shapes the terrain, zones residential, commercial and industrial districts, plops civic buildings and landmarks, runs power and water networks, and legislates the city through ordinances and taxes. The design and simulation lean toward the depth and tile-driven statistical feel of SimCity 4, with modern conveniences and visible agent flavor inspired by Cities: Skylines.

The headline fantasy is being a capable, slightly nerdy mayor — the player who actually reads the budget screen, tweaks tax sliders, and watches a town mature into a metropolis. Growth is paced by population-gated unlocks, so each milestone opens new tools and new problems.

**3. Design Pillars**

- **Depth over fidelity.** Rich interlocking systems (ordinances, taxes, utilities, zoning tiers, ploppables) over photoreal visuals.

- **Readable stylized world.** Low-poly, warm palette; the camera stays pulled back, so silhouettes and color drive readability.

- **Satisfying growth.** Clear unlock progression keyed to population milestones; each tier reshapes the playspace.

- **Policy matters.** Ordinances and taxes are real levers with stat effects and trade-offs — not flavor toggles.

- **Data-driven and modder-friendly.** Content defined via ScriptableObjects; modding is a post-launch first-class citizen.

- **Solo-coder friendly scope.** Statistical core avoids the agent-based performance wall; budgets are protected by hard MVP/V1/stretch gating.

**4. Target Audience & Platform**

**Primary audience:** city-builder fans aged roughly 20–55 who enjoyed SimCity 3000/4 and Cities: Skylines, and who want something a little more systems-driven than modern cozy builders. Single-player only.

**Platform:** PC Windows as primary. Linux and macOS builds are nice-to-haves via Unity's cross-compilation, gated on testing bandwidth.

**Input:** mouse + keyboard. Controller support is a stretch goal.

**Localization:** English at launch. Design UI strings externalized from day one to make later localization cheap.

**5. Scope & Release Model**

Hobby project for now, with a possible pivot to a small commercial indie release if the team is happy with the result. That shapes scope decisions: the project should be shippable as a free hobby build at the end of each major milestone, and every milestone should leave the codebase in a commercially viable state (licensed assets, clean IP, no rights-encumbered content).

No external funding assumed. No hard deadline. Scope is controlled by strict MVP/V1/stretch gating rather than by hours spent.

**6. Core Gameplay Loop**

Minute to minute:

- Assess the city's state via info overlays (demand, pollution, service coverage, budget).

- Plan: decide what is needed — more zoned land, a new power plant, a policy change, a road upgrade.

- Build: terraform, lay roads and utilities, paint zones, plop service buildings, pass ordinances, adjust taxes.

- Simulate: unpause and watch the city tick.

- React: fix problems revealed by the sim — congestion, abandoned zones, budget shortfalls.

- Unlock: population milestones open new tiers, new ordinances, new building types.

- Repeat at a larger scale.

**7. Feature List**

Features are tiered into MVP (vertical slice), V1 (threshold for a commercial release), and Stretch (post-launch or if budget allows). Tier changes require explicit team agreement.

**7.1 Feature Priority Table**

|  |  |  |
|----|----|----|
| **Feature** | **Tier** | **Notes** |
| Free-look camera (pan, zoom, rotate) | **MVP** | Pulled-back RTS-style camera with smooth pan and angle limits. |
| Heightmap terrain + basic terraforming | **MVP** | Raise, lower, flatten brush tools. |
| Road tool (straight and curved) | **MVP** | Snapping, intersections, tier 1 road only. |
| Zoning brush (R / C / I, low density) | **MVP** | Paintable zones; demand-driven growth. |
| Statistical growth model | **MVP** | Tile-based; spawns visual buildings. |
| Power network (plant + cables) | **MVP** | Graph-based connectivity; one plant type. |
| Water network (pump + pipes) | **MVP** | Graph-based reachability; one pump type. |
| Budget (income from tax, expenses from services) | **MVP** | Monthly tick. |
| Save / load | **MVP** | JSON or binary with schema version. |
| Full terraforming (textures, slope, flatten tools) | **V1** | Paint terrain types; area tools. |
| Bridges and tunnels | **V1** | Automatic bridging over water and gaps. |
| Ploppables: fire, police, hospital, school, park, landmark, recycling | **V1** | Each with coverage radius and upkeep. |
| Service coverage system | **V1** | Overlay per service type; affects desirability. |
| Ordinances | **V1** | Toggleable laws with monthly cost and stat modifiers. |
| Taxes (slider per R/C/I) | **V1** | 0–20% range; affects demand. |
| Population-gated unlock tree | **V1** | Village / Town / City / Metropolis tiers. |
| Tutorial + onboarding | **V1** | Scripted first ~20 minutes. |
| Demand meter (R/C/I) | **V1** | Drives zone growth. |
| Visual agent flavor (capped pedestrians + vehicles) | **V1** | Reflects stats, doesn't drive them. |
| Time controls (pause, 1x, 2x, 3x) | **V1** | Standard city-builder controls. |
| Day/night cycle (visual) | **V1** | Optional toggle. |
| Medium and high density R/C/I zones | **V1** | Unlocked through tiers. |
| Multiple road tiers (street, avenue, highway) | **V1** | Gated by unlocks. |
| Mass transit (bus, metro) | **Stretch** | Post-launch. |
| Steam Workshop / mod support | **Stretch** | Locked as post-launch. Asset bundles + ScriptableObject overrides. Structure the content registry and Addressables upfront so the eventual lift is minimal. |
| Disasters | **Stretch** | Fire, flood, power surge. |
| Districts and per-district policies | **Stretch** | Named areas with local ordinances. |
| Multiple biomes | **Stretch** | Desert, tundra, temperate. |
| Weather + seasons | **Stretch** | Visual and minor stat effects. |
| Region / neighbor trade | **Stretch** | SimCity 4-style regional play. |
| Controller support | **Stretch** | Steam Deck verification goal. |

**8. Simulation Model**

The simulation is a hybrid: a statistical core drives all gameplay-relevant numbers, while a visual agent layer provides the perception of a living city. This fits the SimCity 4 lean, keeps the engineering budget tractable for a solo coder, and leaves the door open to upgrade specific systems to real agent-based models in the future (traffic is the most likely candidate).

**8.1 Statistical Core**

- Simulation / zoning grid: 8 m cells. Industry-standard (Cities: Skylines) and immediately readable to players; a small house is 2×2 cells, a larger building 4×4 to 4×6.

- Terrain heightmap: 4 m vertex spacing, decoupled from the sim grid. Keeps terraforming smooth without forcing the sim to pay for that resolution.

- Tile size is exposed as a single compile-time constant referenced everywhere — not baked into gameplay code — so preproduction can playtest 6 m or 10 m with a one-line change.

- Each tile stores: elevation, terrain type, pollution, land value, desirability, service coverage per service, utility coverage (power, water), zone type, density cap.

- Zoned tiles compute population, jobs, and wealth each growth tick based on local tile stats and global demand.

- Zone growth spawns a visual building of the appropriate density/wealth tier from the art kit.

- Utilities are graph problems: power flow over a cable graph, water reachability over a pipe graph. Solved per network, not per tile per frame.

- Traffic is abstracted as road-segment congestion: each segment has capacity and a load derived from nearby residents and jobs. Congestion reduces desirability and service coverage effectiveness.

**8.2 Visual Agent Layer**

- Hard caps on visible pedestrians (target ~200) and vehicles (target ~500) regardless of population.

- Agents spawn from plausible sources (houses in the morning, shops at lunch, industry on shift change) and path to plausible destinations via the road network.

- Despawn offscreen or on arrival. Agents do not affect simulation stats — they reflect them.

- LOD: agents beyond a distance threshold are not spawned at all. No billboards, just gaps.

**8.3 Tick Model**

- Core sim tick: 1 Hz (configurable).

- Growth / demand tick: 0.1 Hz (once every 10 seconds).

- Budget tick: once per in-game month.

- Time scale: 1x / 2x / 3x speeds multiply tick rates; pause stops ticks but keeps visual agents animated at idle.

**9. Content Systems**

**9.1 Zoning**

- Residential: low / medium / high density.

- Commercial: low / medium / high density.

- Industrial: base tier; specialization (manufacturing, logistics, agriculture) considered for late V1 or stretch.

- Zones are painted with a brush; the growth model decides what actually spawns.

**9.2 Utilities**

- Power plants: coal (cheap, polluting), wind (clean, intermittent-flavored), solar (clean, expensive), nuclear (endgame unlock).

- Power distribution: along roads by default; stand-alone cable tool for outlying areas.

- Water: pumps and towers for supply; pipe network for distribution; sewage treatment as a counterpart.

- Garbage / recycling: collection-range building; stretch: landfill vs incinerator vs recycling trade-off.

**9.3 Roads and Transport**

- Straight and curved road tool with snapping and intersection handling.

- Road tiers: dirt, street, avenue, highway — gated by population unlocks.

- One-way roads.

- Bridges and tunnels, auto-suggested when the tool spans water or elevation.

- Mass transit (bus, metro) in stretch.

**9.4 Services (Ploppables)**

- Fire station, police station, hospital, clinic.

- Schools: elementary, high school, university (university in late V1).

- Parks: small, medium, plaza; landmarks (ploppable, cosmetic + desirability bonus).

- Recycling plant, garbage dump.

- Each ploppable has: cost, monthly upkeep, coverage radius, stat effects on tiles inside the radius.

**9.5 Ordinances**

Ordinances are toggleable laws. Each has a monthly cost (usually), a set of stat modifiers, and sometimes prerequisites. Designed to be data-driven — new ordinances should be addable via ScriptableObject without code changes.

- Examples at launch: Free Public WiFi, Smoking Ban, Recycling Program, Pro-Reading Campaign, Clean Industry Standard, Neighborhood Watch, Juvenile Curfew, Small Business Grants, Safety Inspections.

**9.6 Taxes**

- Slider per zone type (R, C, I); 0–20% typical range.

- Higher rates increase short-term income but reduce demand and increase abandonment risk.

- Optional: per-density tax tiers (tax R-high more than R-low) — decide in V1.

**9.7 Unlock Tree**

- Gated by population milestones: Village (500), Town (2k), Small City (10k), City (50k), Metropolis (150k+).

- Each tier unlocks new buildings, new ordinances, new zoning densities, new road tiers.

- Unlocks are permanent per save and visible on a tree screen so the player always knows what's next.

**10. Art Direction**

- Stylized low-poly. Flat-shaded or soft cel-shaded; decided by style frames in preproduction.

- Warm, high-readability color palette. UI echoes the palette.

- Reference mood: Townscaper warmth + SimCity 4 readability + Mini Metro confidence.

- Two LODs per building; the camera rarely sits close enough to demand three.

- Modular building kit: one set of base pieces (walls, roofs, doors) recombines into many buildings. This multiplies the modeler's output.

- Animation: minimal. Flags flap, water shimmers, night windows glow — no full character rigs.

**11. UI / UX**

- Bottom toolbar for tools (terraform, road, zone, plop, utilities, ordinances, taxes).

- Right-side stats and budget panel, collapsible.

- Overlays for pollution, desirability, traffic, coverage, land value.

- SimCity 4-style line graphs for long-term stats.

- Accessibility: colorblind-safe overlay presets, scalable UI text, remappable keybinds from day one.

- Tooltip-first learning. Tutorial is opt-in and scripted, not a tutorial level — keep the first city the player's own.

**12. Technical Architecture and Tools**

**12.1 Engine and Runtime**

|  |  |
|----|----|
| **Engine** | Unity (latest LTS — e.g., Unity 6 LTS as of 2026) |
| **Language** | C# (.NET Standard 2.1+) |
| **Render pipeline** | Universal Render Pipeline (URP) — fits stylized low-poly, scales across hardware |
| **Input** | Unity Input System (new input system) |
| **UI** | UI Toolkit (UIElements) for data-bound, maintainable UI |
| **Simulation code** | Managed C# first; opt-in Jobs/Burst for heavy systems (utility graph, zone growth). DOTS/ECS is on the table for specific hotspots only, not blanket architecture. |
| **Pathfinding (visual agents only)** | A\* Pathfinding Project (Asset Store) or Unity NavMesh — prototype both |
| **Save format** | Binary with JSON fallback for debug; schema version field on every save |

**12.2 Data and Content**

- ScriptableObjects for all content definitions (buildings, ordinances, zones, ploppables, unlock tiers, taxes).

- A single "content registry" SO holds references; content can be swapped and extended by addon packs.

- Addressables for asset loading; sets the stage for later mod support.

**12.3 Source Control and Collaboration**

|  |  |
|----|----|
| **Version control** | Git + Git LFS (models, textures, audio) |
| **Host** | GitHub (private) — switch to paid tier or GitLab if LFS bandwidth becomes an issue |
| **Branching** | Trunk-based with short-lived feature branches; main must always build |
| **Commits** | Conventional commits (feat:, fix:, chore:) for cleaner history |
| **Project management** | GitHub Projects or Notion — lightweight kanban |
| **Communication** | Discord server; voice calls weekly |
| **Issues** | GitHub Issues; priority labels mirror the MVP/V1/Stretch tiers in this doc |
| **Code review** | PRs required for anything touching shared systems; solo can self-merge prototypes |

**12.4 Art Pipeline**

- Modeling: Blender (free, indie standard).

- Exchange format: FBX or glTF with authored LODs.

- Texturing: hand-painted / gradient textures; low-resolution is fine.

- Naming conventions: documented in a separate Art Bible doc before the first kit ships.

- Asset review: everything goes through a shared "new_assets" folder and is only promoted after a programmer-modeler pass.

**12.5 Audio Pipeline**

- SFX from licensed libraries (freesound CC0, Unity Asset Store) to start.

- Music commissioned or licensed when the prototype proves fun.

- Middleware: start with built-in Unity audio; move to FMOD (free indie license) only if mixing complexity demands it.

**12.6 Build and Release**

- CI: GitHub Actions running headless Unity builds for Windows each main merge.

- Distribution during hobby phase: itch.io (free, friendly, good for playtest drops).

- If commercial: Steamworks SDK integration, Steam page, demo build, wishlist campaign.

- Crash reporting: Unity Cloud Diagnostics or Sentry (opt-in, disclosed).

**12.7 Tooling Wish-list (in-engine)**

- Sim debug overlay: per-tile and per-building inspector accessible in play mode.

- Time-travel save-scrubbing (snapshot every minute) for bug reproduction.

- Deterministic seedable simulation so bugs are reproducible from a save + seed.

- CLI-style command console for cheats and debugging.

**13. Team and Roles**

The team is two people.

- **Mark — Programmer / Lead.** All code, systems, integration, tooling, build, UI implementation, audio integration, design decisions. Final call on scope.

- **3D Modeler.** Buildings, props, vehicles, landmarks, LODs, Art Bible compliance.

Practical implications of a two-person team:

- The programmer wears every non-modeling hat. Lean heavily on Asset Store and open-source packages for anything non-core (pathfinding, UI primitives, save/load utilities, audio middleware) so coding time stays on the gameplay systems that are actually differentiating.

- Art bottleneck is the dominant production risk. The modular building-kit approach in Section 10 is not optional — it is how the modeler produces a city's worth of variety without burning out.

- No dedicated designer, QA, or audio person. Plan balance passes, playtests, and audio polish as explicit milestones rather than assuming they happen in the background.

- If the project gains momentum and additional collaborators join later, the highest-leverage roles would be (in order): tuning/balance designer, UI/UX designer, audio designer. Add only when a clear bottleneck justifies it.

**14. Roadmap**

Durations are effort estimates for a part-time team. The hobby framing means these are not deadlines; they are order-of-magnitude targets to keep scope honest.

|  |  |  |
|----|----|----|
| **Phase** | **Approx. Duration** | **Goals** |
| **Preproduction** | 2 months | Style frames approved (2–3). Tech spikes: grid + zone growth prototype, road tool prototype, utility graph prototype. Repo + LFS setup. Art Bible v0.1. Working title finalized. |
| **Vertical slice** | 4 months | MVP feature list fully playable. One biome. One building kit. A 20-minute session end-to-end: terraform, road, zone, plop power, grow to 500 population. |
| **Alpha** | 6 months | All V1 features present but rough. UI, tutorial skeleton, save/load, balance pass 1. Internal playtests weekly. |
| **Beta** | 6 months | Polish, balance, bug fixing. External playtesters. Trailer. Store page if commercial. Feature freeze. |
| **Release candidate** | 1–2 months | Gold-master candidate builds, launch prep, marketing push if commercial. |
| **Post-launch** | Ongoing | Patches, modding support, stretch features (transit, disasters, districts). |

**15. Industry Standards and Benchmarks**

- Engine: Unity dominates indie city-builders (Cities: Skylines 1 + 2, Workers & Resources, many smaller titles). Unreal is rarer in the genre; Godot is emerging.

- Simulation: modern AAA city-builders are agent-based via DOTS or custom ECS. A well-tuned statistical or hybrid simulation remains commercially viable — see Tropico, SimCity 4's lasting relevance, and smaller recent releases.

- Pathfinding: flow fields for aggregated flows; A\* for individual agents. Mixed approaches are standard.

- Performance targets: stable 60 FPS on mid-range hardware at release; 30 FPS floor on minimum spec. CPU is almost always the bottleneck, not GPU.

- Save files: expect 1–20 MB at medium city size; binary with schema versioning is standard. Plan for saves 10x larger than today because cities grow.

- Modding: Steam Workshop is the baseline expectation for PC city-builders. Data-driven content + asset bundle loading is the delivery mechanism.

- Release cadence: indie city-builders typically ship via Early Access with a 12–24 month EA window, then a 1.0. This matters only if the project goes commercial.

- Team size: commercial indie city-builders ship with teams of 2–10. Cities: Skylines 1 famously shipped with about 13. You are well within normal range.

**16. Risks and Mitigations**

- **Solo coder bandwidth.** Mitigation: strict tier gating; prefer statistical sim; lean on Asset Store for non-core systems (pathfinding, audio, UI primitives).

- **Scope creep.** Mitigation: any tier change requires explicit team sign-off; all ideas go to a "post-launch" backlog by default.

- **Art bottleneck.** Mitigation: modular building kit; share textures aggressively; accept "90% good" for most buildings.

- **Performance (late-stage).** Mitigation: profile from month 2; cap visible agents; make tick rate configurable; keep sim deterministic so reproducing bugs is cheap.

- **Team attrition.** Mitigation: data-driven content means a modder or designer can contribute without blocking on coder time; document systems as they ship.

- **Engine upgrades.** Mitigation: pin to a Unity LTS version for each phase; only upgrade at phase boundaries.

- **IP / asset contamination.** Mitigation: keep a simple "asset register" spreadsheet tracking every external asset, its license, and source URL. Saves the project if the hobby build goes commercial.

**17. Open Questions**

- Working title: CityRise (locked as of v0.1). Final title revisit before any commercial release; confirm no trademark conflict.

- Grid tile size: locked at 8 m simulation/zoning grid, 4 m heightmap. Revisit only if preproduction prototyping flags a problem.

- Day/night cycle in V1 or stretch? Cheap visually but expensive for QA on overlays.

- Mod support: locked as post-launch. The content registry and Addressables should still be structured with modding in mind so the post-launch lift is minimal.

- Visible agents in MVP or V1? A tech test in preproduction will answer.

- Team composition: locked at two (programmer + 3D modeler). Future collaborators deferred until a specific bottleneck justifies the hire.

- Commercial intent go/no-go: decide at end of Alpha based on how the game feels.

**18. Appendix: First Actionable Steps**

If you want to convert this document into motion, here are the first concrete tasks in order:

- Spin up the Git repo with LFS, add a .gitignore for Unity, commit an empty Unity project on the chosen LTS.

- Working title locked: CityRise. Quick trademark + domain check; reserve a placeholder Twitter/Bluesky/itch handle.

- Write a 1-page Art Bible v0.1: palette swatch, silhouette rules, LOD policy, modular kit sketch.

- Prototype 1: grid + zone paint + statistical growth spawning primitive cubes as "buildings."

- Prototype 2: road tool with curves and snapping.

- Prototype 3: utility graph (power) over road-borne conductance.

- Share the three prototypes with the team; if all three feel right, start the vertical slice.

- Lock this GDD as v1.0 after preproduction; revise it at the end of each phase, not ad hoc.
