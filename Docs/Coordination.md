# AMJ Waterworks Coordination

### PRIORITY-FOREST-FOODS-20261008 — subsequent feature sequencing

**Owner:** Waterworks release/priority coordination  
**Status:** PROJECT EXECUTION ORDER RECORDED; v1 existing-save E2E STILL IN PROGRESS

Author placed Japan-appropriate native fruit trees / edible forest foods in
the Project **after Ironmaking, before the next Waterworks/Rice
feature-development phase**. This does not demote historical Waterworks/Rice
Reconstruction P0, change Waterworks v1's approved canal behavior or revoke
its optional integration with Rice Cultivation. See Project
`Docs/ImplementationPriorities.md` and `Docs/Roadmap.md` for execution order.

**Finish the bounded current Waterworks v1 work**, especially the ongoing
separate two-phase save-before-install -> Waterworks-enabled add-to-save E2E
and its real runtime ERROR gate. Do not abandon already-built code, report
that compatibility passed prematurely or broaden v1 in order to overtake the
forest-food work. Subsequent feature expansion / Rice integration waits for
its later roadmap slot. Existing detailed E2E notes remain authoritative.


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
**Status:** IN PROGRESS — uncompiled prototype source checkpoint; build/runtime pending

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

### DOC-SHARED-RULES-OWNER-001 — Shared rule migration to Project (2026-10-08 JST)

**Owner:** Project common rules / repository routing
**Status:** DONE — current AGENTS and shared-rule references route to Project

Canonical shared rules and Workshop template/tooling now live in Project `Docs/SharedRules.md` and its linked sources. Grains old Markdown paths are migration pointers only. Existing historical coordination entries retain their original commit/path provenance; resolve future work through the new Project index. Mod-specific implementation, tests and accepted content art remain with this repository. No runtime behavior, new preview generation or Steam publication is part of this migration.


### PROTO-WATERWORKS-002 — source checkpoint

**Owner:** Waterworks
**Status:** IN PROGRESS — source only, not compiled/runtime-tested

The baseline source prototype contains metadata, dry/wet TerrainDefs with Vanilla water/soil paths, designators, Construction jobs, graph state, natural source checks, save/restore, a csproj, and a static audit script. No imagery has been generated/copied and no public release is authorized.

Next: compile against actual RimWorld 1.6 DLLs; run static audit and isolated Pickle/RimTest Redux runtime tests; inspect bridge/foundation coexistence and shader behavior. Require zero Waterworks ERROR entries before closing.


### PROTO-WATERWORKS-API-001 — RimWorld 1.6 draw-style compatibility

**Owner:** Waterworks implementation
**Status:** DONE — source correction committed; build/runtime verification still required

- Audited RimWorld 1.6 `Verse.Designator`, `DesignatorManager` and `DrawStyle_Line`: `DraggableDimensions` is a removed/obsolete API.
- The dig/fill designators now expose the dedicated `AMJW_CanalLine` category with only Vanilla `Line` style; single-cell and cardinal straight drag are retained.
- A static contract check guards against reintroducing the old property.
- This is an **API-source audit**, not a successful compilation or in-game validation.


### TEST-WATERWORKS-STATIC-001 — repeatable source validation

**Owner:** Waterworks implementation  
**Status:** DONE for static/build tooling; runtime verification remains OPEN

- Enhanced `Tests/static_audit.py` with loaded class references, DefOf bindings, localization and the 1.6 draw-style regression check.
- Added `Scripts/validate-source.ps1` for non-interactive static + Windows RimWorld DLL build.
- Canonical repository procedure: `Docs/GoldenPaths/SourceValidation.md`.
- Unmet gates: actual 1.6 C# build, non-interactive Pickle/RimTest Redux, runtime ERROR=0, save/load and actual wet-water shader/bridge appearance.


### PROTO-WATERWORKS-EVENT-001 — terrain-change invalidation, narrow visual intent

**Owner:** Waterworks implementation
**Status:** DONE for source-level design/implementation; build/runtime still pending

- Use RimWorld 1.6 `MapEvents.TerrainChanged` notifications and coalesce them to the next map tick; no periodic full-map scan.
- Maintain a per-map set of Waterworks canal cells; rebuild it during map initialization/load and update on excavation/fill and external changes.
- Prevent graph recalc from recursively reacting to its own wet/dry TerrainDef changes.
- Broad terrain-change invalidation supports standing-water size changes away from an adjacent canal.
- Image production is on hold; when revisited, render the excavated center channel substantially **narrower** than the one-cell terrain footprint. The previous wide concept images are not accepted production art.
- Remaining acceptance: game build, Pickle/RimTest Redux, no runtime ERRORs, and water/bridge shader rendering.


### PROTO-WATERWORKS-VEGETATION-001 — wild-plant prerequisite

**Owner:** Waterworks implementation
**Status:** DONE for source-level change; build/runtime pending

- The dig designator allows natural soil with ordinary wild plants; it does not auto-remove sown crops.
- Before digging, the existing Construction work giver uses Vanilla `CutPlant` for a removable wild plant and waits until the cell is clear. It respects forbidden/pawn unwilling-to-cut behavior.
- The final terrain replacement still rejects occupied plant cells, avoiding silent crop/tree destruction.
- A static audit protects the split between placement eligibility and actual excavation.
- Test in RimWorld with wild grass/trees, protected plants, cultivated fields, and cases where cutting is forbidden.


### PROTO-WATERWORKS-JOB-001 — work-order validity throughout execution

**Owner:** Waterworks implementation
**Status:** DONE in source; game-compile and runtime assertions remain pending

- Construction work givers revalidate Dig/Fill eligibility before assigning jobs.
- Vanilla `JobDriver_AffectFloor` consumes its designation after calling `DoEffect`; the canal job driver therefore installs a fail condition that checks eligibility while work is running, preventing a stalled/invalid cell from losing its work designation as a silent no-op.
- Wild-plant cutting remains a prerequisite, whereas actual excavation requires the cleared cell.
- Regression markers added to `Tests/static_audit.py`; verify with actual runtime that adding a floor/bridge during a pending or in-progress earthwork leaves the designation recoverable.


### PROTO-WATERWORKS-TERRAIN-001 — Vanilla Diggable compatibility

**Owner:** Waterworks implementation
**Status:** DONE — source guard corrected; runtime validation still pending

- Removed the implicit requirement that `TerrainDef.natural` be true: Vanilla standard natural soil XML does not require that field.
- The authoritative eligibility contract is `Diggable` plus explicit exclusions; synthetic constructions/roads/water remain rejected.
- Added a static regression check to prevent this accidental all-soil rejection returning.
- Next runtime acceptance includes soil, rich soil, gravel, sand, marshy soil, and AMJ Environment `AMJ_ThinSoil`.


### BUILD-WATERWORKS-001 — missing Diggable DefOf member (2026-10-08)

**Owner:** Waterworks implementation
**Status:** DONE — author reran static audit and real C# build successfully (0 warnings, 0 errors)

The author's `Scripts/validate-source.ps1` completed its existing static contract check but C# build failed with CS0117 at `Source/CanalMapComponent.cs`: `TerrainAffordanceDefOf.Diggable` is not defined in RimWorld 1.6.

Confirmed in the decompiled 1.6 `RimWorld/TerrainAffordanceDefOf.cs`: the class has no `Diggable` member; the terrain XML still defines the `Diggable` affordance. Fix uses `TerrainDef.affordances.Exists(...defName == "Diggable")`, not a nonexistent DefOf symbol. Updated `Tests/static_audit.py` to reject the old reference and require the loaded-Def lookup. Clarified `NaturalTerrainBase`'s `natural=true` inheritance in the formal design.

**Result:** the author reported the static audit and build passing in 2.29 seconds; next gate is actual isolated Pickle runtime.


### TEST-WATERWORKS-E2E-001 — isolated Pickle/Quickstarts core verification

**Owner:** Waterworks implementation
**Status:** DONE — author confirmed no errors in initial 4/4 suite (2026-10-08 JST)

Following the user's clean production build, a first developer-only Pickle/Quickstarts suite now covers:
- production Def loading and Waterworks/Vanilla Bridge affordances;
- cardinal canal connectivity including diagonal-only rejection, splitting and reconnection;
- 8 versus 9 standing-freshwater cells and explicit ocean/marsh rejection;
- actual Vanilla Bridge foundation preservation of wet/dry canal topology and Gravel restoration.

`Scripts/run-e2e.ps1` builds test assemblies, stages `AncientMedievalJapanWaterworks.E2E` separately and creates isolated test savedata. It must not alter the player's normal ModsConfig. A pass requires all **4/4** named Pickle scenarios and `Player.log` with **zero [ERROR] lines**.

**Observed:** the author reported no errors and only the expected pending-gates warning after the runner. The four loaded-map scenarios are accepted. Subsequent pawn-job coverage is tracked in `TEST-WATERWORKS-E2E-002`; persistence and visual checks remain pending. Art remains deferred.


### TEST-WATERWORKS-E2E-002 — real pawn earthwork jobs

**Owner:** Waterworks implementation
**Status:** DONE — author confirmed expanded 5/5 suite passed (2026-10-08 JST)

Following the author's passing 4/4 suite, the fifth scenario checks a capable real Construction pawn, actual map designations, production WorkGiver-generated Dig and Fill jobs, pawn JobTracker execution with engine ticks, wet canal state after excavation, restored Soil after filling, and removal of completed designations. It does not call the direct `CanalMapComponent.Dig/Fill` methods as the operation under test.

The runner and static feature contract now require **5/5** exact scenarios and zero isolated `[ERROR]` entries. The test remains in the isolated Quickstarts/Pickle mod; the player's normal mod settings are not altered.

**Observed:** user reported passing the expanded suite without errors. Production Construction-pawn Dig/Fill E2E is accepted. The next persistence gate is TEST-WATERWORKS-E2E-003; visual presentation remains OPEN.


### TEST-WATERWORKS-E2E-003 — real .rws round trip and original-ground persistence

**Owner:** Waterworks implementation  
**Status:** DONE — author confirmed 6/6 suite passed (2026-10-08 JST)

Following user-confirmed **5/5**, the new scenario creates a wet canal originally dug from Gravel and an isolated dry canal originally dug from Soil, uses Pickle's own built-in `When I save and reload` engine step to write and reload an actual `.rws`, then obtains a **new** `Find.CurrentMap.GetComponent<CanalMapComponent>()` and checks:
- wet/dry terrain states rebuilt after reload;
- original-terrain records are deserialized (`CanFill` accepted for each);
- a source change after reload dries/rewets the connected canal through `TerrainChanged`;
- filling the loaded canals restores the distinct original Gravel and Soil TerrainDefs.

The original Quickstart map belongs only to isolated TestResults SaveData; Pickle's round-trip save is temporary and removed by Pickle. E2E requires **6/6** exactly named scenarios with zero isolated `[ERROR]` entries.

**Observed:** the author reported the expanded 6/6 suite passed. Real .rws save/reload and original-terrain restoration are accepted. Visual presentation remains OPEN, and separately verifying installation into a save made without Waterworks is tracked by `TEST-WATERWORKS-E2E-004`. No new imagery is authorized.


### TEST-WATERWORKS-E2E-004 — load save created before Waterworks installation

**Owner:** Waterworks implementation
**Status:** IN PROGRESS — two-cold-start test suite staged; not built or runtime-tested yet

1. Phase A boots Core/Harmony/Pickle/Quickstarts plus only the Vanilla bootstrap test Mod. It explicitly asserts that neither Waterworks TerrainDef exists, creates an isolated Vanilla Quickstart map, prepares Soil/Gravel beside natural flowing freshwater, and uses Pickle `When I save and reload as "waterworks-before-install"` to leave a genuine `.rws` under scratch `TestResults/AddToSave/SaveData/Saves`.
2. Phase B cold-starts a fresh RimWorld process with Waterworks enabled using **the same savedata**; its `Given the save file "waterworks-before-install" is loaded` step loads that earlier save. It verifies ordinary Vanilla terrain was untouched by installation, excavates wet/dry canals, then uses Pickle's real save/reload again and verifies restoration to both Soil and Gravel.
3. `Scripts/run-add-to-save-e2e.ps1` is a **separate** gate from the existing confirmed 6/6 suite. Each phase requires its exact 1/1 named scenario and isolated runtime ERROR count 0. Normal `ModsConfig.xml` and user saves are never modified.

**Next gate:** run the new script locally, fix any test-only API/PowerShell/RimWorld integration errors, and mark DONE only after both phases pass. This does **not** establish compatibility with a full user modpack. Image creation is still deferred.


### TEST-WATERWORKS-E2E-004-PS-001 — PowerShell interpolation parser failure

**Owner:** Waterworks implementation  
**Status:** FIXED IN SOURCE — rerun of the two-phase test pending

The author attempted `Scripts/run-add-to-save-e2e.ps1` and PowerShell stopped at script parse-time (line 139) with `InvalidVariableReferenceWithDrive`: the string `"[OK] $phase: 1/1 ..."` was misparsed because the colon immediately followed the variable name. The script has been changed to `"[OK] ${phase}: 1/1 ..."`.

To avoid repeating this class of avoidable failure, `Scripts/validate-source.ps1` now runs the PowerShell AST parser over every `Scripts/*.ps1` before static validation, C# building, or game startup. `Tests/static_audit.py` requires the corrected string and the AST preflight.

**Acceptance still pending:** the two-phase existing-save E2E must actually run successfully (Phase A 1/1, Phase B 1/1, no runtime ERROR). This syntax fix is not evidence of a passing E2E.


### TEST-WATERWORKS-E2E-004-ERROR-001 — Phase B emits one runtime ERROR

**Owner:** Waterworks existing-save acceptance
**Status:** BLOCKED — specific ERROR content not yet supplied; Phase B acceptance remains OPEN

After the PowerShell parser fix, the author reran the isolated existing-save test. The Phase B `AfterInstall` runner reached its runtime error gate and stopped: **one `[ERROR]` line** in `TestResults/AddToSave/Reports/AfterInstall/Player.log`. This proves the earlier parser blocker was bypassed but is **not** a passing two-phase test. The author message includes only the runner's summary, not the actual underlying Player.log error, so no root cause can yet be assigned. The previously verified standalone **6/6** core E2E is unaffected.

The runner now prints each ERROR entry with 12 following context/stack-trace lines (up to five entries) before aborting, while continuing to enforce ERROR=0. The existing log is the primary next diagnostic source; a repeat run is **not** required just to recover the original error. The next step is to inspect the first real ERROR message and stack, determine whether it belongs to Waterworks production, the upgraded-save test fixture, Pickle/Quickstarts, or third-party services, and then change the actual owner code/test if needed. Do not weaken the error gate or claim compatibility before Phase A and Phase B are both clean.


### TEST-WATERWORKS-E2E-004-ERROR-002 — empty test Mod content (2026-10-08)

**Owner:** Waterworks existing-save acceptance
**Status:** FIXED IN SOURCE — two-phase runtime rerun pending

The author supplied the Phase B log: `Mod [DEV] Waterworks Existing Save Integration E2E did not load any content. Following load folders were used:`. The Phase B fixture held only About/Pickle data, and RimWorld 1.6 reported it as an empty mod. Added a test-only `ThingCategoryDef` marker under `Tests/E2E/AddToSaveMod/Defs`, staged it into `Defs/ThingCategoryDefs` by `Scripts/run-add-to-save-e2e.ps1`, and added static regression assertions. This marker is not a production Waterworks Def and has no intended save/gameplay behavior.

Next: run `Scripts/validate-source.ps1` and `Scripts/run-add-to-save-e2e.ps1` on the author's RimWorld installation. Require Phase A 1/1, Phase B 1/1 and zero runtime ERRORs; no acceptance or compatibility claim until then. If a further error occurs, inspect the saved Player.log rather than rerunning only to retrieve its message.


### TEST-WATERWORKS-E2E-004-RESULT — existing-save installation acceptance (2026-10-08)

**Owner:** Waterworks existing-save acceptance
**Status:** DONE — author-supplied isolated reports inspected

The uploaded `Reports(4).zip` includes both cold-start Pickle summaries and Player.log files. Phase A `Vanilla saved game is created without Waterworks`: **1/1 passed**, 0 failed, 0 skipped, 0 `[ERROR]` entries. Phase B `A Vanilla-only saved map safely accepts newly installed Waterworks`: **1/1 passed**, 0 failed, 0 skipped, 0 `[ERROR]` entries. This closes the fixture-content blocker and the dedicated existing-save installation acceptance gate. The previously accepted separate 6/6 suite remains unchanged.

Remaining: visual wet/dry terrain and bridge shader/occlusion inspection; full user-modpack compatibility is not established by isolated acceptance. No new art authorized by this result.
