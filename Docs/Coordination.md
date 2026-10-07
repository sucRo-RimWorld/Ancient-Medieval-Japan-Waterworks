# AMJ Waterworks Coordination

This file is the authoritative coordination surface for work on **AMJ Waterworks**.

At the start of Waterworks work:

1. Read `AGENTS.md`.
2. Read this file from `main`.
3. Read `Docs/Design.md` and any implementation source relevant to the task.
4. Check OPEN / IN PROGRESS items before starting overlapping work.

This file is for handoff, state and blockers only. Confirmed design belongs in `Docs/Design.md` or implementation sources.

## Status vocabulary

- **OPEN** — needs work
- **IN PROGRESS** — being investigated or implemented
- **BLOCKED** — waiting on a prerequisite
- **DONE** — completed and reflected in the formal source of truth
- **ARCHIVED** — historical context only

## Current coordination items

### MIG-WATERWORKS-001 — establish standalone Waterworks repository

**Requested by:** author (2026-10-07 JST)  
**Owner:** Waterworks design / implementation  
**Status:** DONE — standalone repository created and authoritative design migrated from Grains

Result:
- `AGENTS.md` now owns Waterworks repository workflow and permanent boundaries.
- `Docs/Design.md` is the authoritative Waterworks design.
- The prior temporary design in `Ancient-Medieval-Japan-Grains/Docs/WaterworksDesign.md` has been reduced to a migration pointer (`fe4f52759f459472107a31d9e220a076e7e06139`).
- Grains `Docs/Design.md` now points to this repository as the authoritative Waterworks source (`0afa9be3b142ac8a7b77905ae5c63aa726ec57f7`).
- Grains coordination handoff is closed (`418a505c1a01fcdc1443e72dbfbb5a19f9978ea5`).

Initial transferred draft (historical; superseded by current `Docs/Design.md`):
- the migration originally included a dedicated intake building, gate, culvert and source classes in v1;
- the author later narrowed v1 to the direct natural-water canal core in commit `51f43ec6d660bd3c85927951dfe06ce569ac967f`;
- current v1 no longer requires a separate intake building, gate, culvert, hot-spring classification, DBH adapter or public integration API.

Historical source commits in Grains:
- ownership split: `f594c340fad0ee067e7ce07778e68856822402fb`
- Waterworks-only cleanup: `10b13764427dd2abdac916eab1deeba9d07bc6df`
- v1 network baseline: `68ce78db90830fe065399f3385475e36ba0507c6`
- scope/source semantics: `da34186c6bee44774fbe0a70ac1432af77376796`
- construction semantics: `4e51249d8b65ea06bd499ab9d1c3320a11b39bdc`
- progression/integration: `f89cbc2a7ccdbf9667d30d635e38392e85d9cb0f`
- DBH overlap audit: `d612d5402f76af466cf86069e55c6c1d72fb342c`
- terrain/v1 exclusions: `f36e37d8145b13d951e758bb453c17b3667d4a3c`
- dedicated temporary design creation: `79b221786d5d9be66d74dfb0e501b977cba5018c`
- implementation architecture: `770f6b8aeef80d2c1918ef0e853f105fa7fceded`
- DBH water-only compatibility profile: `97e91725edbac96bfe63056afe9be3bee518376d`

**Next action:** implement only the minimal direct-source vertical slice: dig canal -> orthogonal adjacency to valid river/pond terrain -> wet/dry connected-component state -> fill canal. Do not pre-commit to gates, culverts or adapters; add them later only if play or a real consumer demonstrates a need.

### PROTO-WATERWORKS-001 — minimal independent canal prototype

**Owner:** Waterworks implementation  
**Status:** OPEN — repository handoff complete; implementation not started

Scope:
1. use the fixed package/Def identity from `Docs/Design.md` and add the minimal RimWorld 1.6 About/load structure;
3. implement dug-canal TerrainDef plus Dig/Fill canal semantics;
4. implement the audited Vanilla 1.6 source rules: moving freshwater always valid; standing `WaterShallow`/`WaterDeep` requires a 9-cell connected body; ocean/marsh/wet soil/mud invalid;
5. implement Diggable-based excavation eligibility with Ice/water/road/artificial-floor exclusions, including AMJ Environment `AMJ_ThinSoil` without a dedicated patch;
6. use Construction work with initial values 500 dig / 300 fill and canal `pathCost=10`;
7. implement per-map event-driven connectivity state;
8. represent canal state with `AMJW_DugCanalDry` / `AMJW_DugCanalWet` TerrainDefs and switch only on invalidation/load correction;
9. add Architect -> Orders line-drag Dig/Fill designators, Construction jobs and clear invalid-placement feedback;
10. implement bounded standing-water validation that stops once 9 eligible cells are found;
11. automate moving/standing source validity including 9-cell threshold, ocean rejection, orthogonal connectivity, diagonal rejection, marsh/marshy-soil non-source behavior, marshy-soil excavation/restoration, Vanilla-bridge crossing/continuous-cover placement and non-interruption, bridge-safe fill rejection, disconnect/reconnect, fill restoration, save/load and runtime ERROR=0 checks.

Do **not** add gate, culvert, DBH adapter, Hot Springs adapter, stone lining or consumer gameplay until this vertical slice is green.
Implementation identity fixed by design:
- `Ancient & Medieval Japan - Waterworks`
- packageId `sucro.ancientmedievaljapan.waterworks`
- assembly `AncientMedievalJapanWaterworks`
- namespace `AncientMedievalJapan.Waterworks`
- Def prefix `AMJW_`
- no DLC or external-mod hard dependency; avoid Harmony unless proven necessary.



### FUTURE-MOAT-001 — AI-safe dry moat / water moat extension

**Owner:** future defensive-earthworks / fortification workstream  
**Status:** ARCHIVED — responsibility moved outside Waterworks; retain only optional water-supply integration boundary

Confirmed boundary:
- Dry moats, water moats, bridges, traversal rules, raid AI and breach behavior are outside Waterworks ownership.
- A future defensive-earthworks / fortification mod may optionally consume Waterworks supply state.
- Waterworks owns only whether supplied water reaches that consumer; it does not own the defensive terrain or AI behavior.

**Durable source:** `Docs/Design.md`, scope correction commit `e9e529e01c4f4a83fa612bec0bad58875ba858ad`.

**Next action:** none in Waterworks. The unowned candidate is now tracked in `sucRo-RimWorld/Ancient-Medieval-Japan-Project:Docs/Research/DefensiveEarthworksCandidate.md`; revisit there until an owning repository exists.


### DES-REINFORCED-COVER-001 — canal-specific reinforced cover

**Requested by:** author (2026-10-07 JST)  
**Owner:** Waterworks  
**Status:** DONE — design direction confirmed; implementation deferred until after minimal core

Confirmed direction:
- ordinary canal crossings and wooden covers use the Vanilla bridge;
- Waterworks will not become a general bridge/foundation pack;
- if heavier structures must remain above an intact canal, Waterworks may add one canal-specific reinforced cover/foundation, conceptually a stone canal cover / covered channel;
- that reinforced cover is restricted to Waterworks canal use and should preserve the canal TerrainDef/connectivity underneath where the RimWorld 1.6 foundation model permits;
- do not require VFE Architect or another large bridge mod merely for this single Waterworks function; similar mods remain prior art / optional coexistence targets;
- do not build a separate underground culvert grid first. Reconsider a true culvert only if the reinforced-foundation approach fails a demonstrated layout or integration need.

**Durable sources:** `AGENTS.md` commit `ac80fee83508d3c0994cff260cf62a00bb9e61a2`; `Docs/Design.md` commit `d00df5349b8593361265b34e491c5d06df0818e3`.

**Next action:** none before the minimal direct-source canal prototype is green. When heavy-structure crossings become necessary, audit current RimWorld 1.6 foundation/support affordances and prototype only this one canal-specific cover.


### DES-V1-BASELINE-001 — minimal canal v1 design closure

**Requested by:** author (2026-10-07 JST)  
**Owner:** Waterworks  
**Status:** DONE — design baseline closed; prototype implementation may begin

Closed baseline:
- explicit RimWorld 1.6 natural-water source rules, including ocean/wetland exclusion and 9-cell standing-water threshold;
- Diggable-based excavation compatibility with explicit exclusions;
- 500 dig / 300 fill Construction work and `pathCost=10`;
- dry/wet canal TerrainDefs instead of a custom per-cell fluid/render simulation;
- Vanilla bridge reuse for crossings/covers;
- Architect -> Orders line-drag Dig/Fill interaction;
- save/load rebuild and add-to-existing-save test target;
- no DLC or external hard dependency; no Harmony unless implementation proves it necessary.

**Durable source:** `Docs/Design.md`, commits `b473f0a32de1cdd980e9beda829936fa9d298545`, `cfd6469fe22ebcc2462881d9b7f26065834a2113`, `ae41992e9822abafd01804066b68a1dd79ad8225`.

**Next action:** implement `PROTO-WATERWORKS-001` without adding deferred consumers or control systems.


### COMPAT-MOJ-OWNERSHIP-001 — Japanization / Waterworks boundary

**Owner:** Waterworks / Project Japanization architecture  
**Status:** DONE — current Waterworks v1 unchanged

Project-level Japanization architecture confirms:

- MO Watermill remains independent of Waterworks;
- Japanization does not make a wet Waterworks canal a Watermill power prerequisite merely because both involve water;
- Waterworks continues to own no water-wheel/mechanical-power system;
- no Japanization/Waterworks adapter is required for the current v1 core;
- any future watermill/canal interaction must be justified by a real gameplay consumer after Waterworks core is stable.

**Durable source:** `sucRo-RimWorld/Ancient-Medieval-Japan-Project:Docs/Research/MedievalOverhaulJapanizationIntegrationMatrix.md`, commit `5483cc42744ed2652bf7599272c00225668d9963`.

**Next action:** none.
