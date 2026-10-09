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

### ADD-CHANGENOTE-20261008 — Versioned Workshop update notes

**Owner:** Ancient-Medieval-Japan-Waterworks packaging/release
**Status:** SOURCE IMPLEMENTED — dedicated metadata CI pending; gameplay/release gates unchanged

Project `Docs/WorkshopChangenotes.md` now applies here: `About/Manifest.xml`, `About/Changelog.txt` and `About.xml` agree on `0.1.0-dev`. A narrow new workflow checks the metadata and YADA retention without running unowned gameplay tests. These subscriber metadata files have no effect on gameplay, packageId or Mod dependency rules. No Steam publishing or new runtime verification occurred.

### RULE-AUDIT-20261008 — operating-rule consolidation

**Owner:** Project common rules; this repository retains its local specification and gates.
**Status:** SOURCE RESTRUCTURED; validation/publication evidence is recorded in Project `Docs/RuleAudit.md` and actual commit/CI results, not inferred here.

AGENTS now routes through Project `Docs/SharedRules.md` stop conditions and task procedures. New development requires VE and non-VE source/evidence comparison plus a justified implementation decision. Static/runtime/specification/distribution/publication remain separate states. Historical records below/above retain their original scope; this entry does not reopen paused work, change gameplay/dependencies/art/versions, or supersede owner runtime/release blockers. Main-only Coordination means one authoritative integrated log, not deleting branch snapshots. No Steam/2game update is claimed.


### VIS-WATERWORKS-022 — accepted 16-mask relief integrated and captured (2026-10-09)

**Owner:** Waterworks rendering
**Status:** LOCAL IMPLEMENTATION / RENDERED TEST PASS; author in-game visual review and publication remain separate.

Author selected the original 16-mask dry set (not the narrowed experiment), and requested existing Core water beneath common relief with no wet PNGs. Exact masters/derivatives now live under Art/Sources and Textures/Terrain/AMJW/Canal. Design section 8.0.6 and Docs/GoldenPaths/CanalOverlayRendering.md are the current source of truth. Earlier VIS-001..021 iteration records are superseded by those sources and retained in Git history; the old EW debug generator is not the selected runtime asset path.

SectionLayer redraws stored original terrain, clips Core WaterShallowRamp to generated bed geometry, then overlays the accepted transparent relief. A detected opaque-base draw-order failure was repaired before final captures. The final private-desktop visual run TestResults/Visual/20261009-102859 passed 1/1 with runtime ERROR=0, producing connected/disconnected/restored and all16-dry captures on real terrain. Core regression passed 7/7 with runtime ERROR=0 including construction, 2x2 exclusion and save/reload. Asset checks passed exact hashes, PNG integrity and 256 compatible border comparisons. Tests use separate profiles; no normal save/config edits.

No full animated river-depth shader, historical per-cell tint/pollution reproduction or Workshop publication is claimed. Current work is on codex/canal-overlay-render; no remote push/merge is implied by this local record.
