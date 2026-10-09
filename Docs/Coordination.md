# AMJ Waterworks Coordination

### PRIORITY-FOREST-FOODS-20261008 — subsequent feature sequencing

**Owner:** Waterworks release/priority coordination  
**Status:** PROJECT EXECUTION ORDER RECORDED; bounded v1 core/runtime gates PASS, visual work remains

Author placed Japan-appropriate native fruit trees / edible forest foods in
the Project **after Ironmaking, before the next Waterworks/Rice
feature-development phase**. This does not demote historical Waterworks/Rice
Reconstruction P0, change Waterworks v1's approved canal behavior or revoke
its optional integration with Rice Cultivation. See Project
`Docs/ImplementationPriorities.md` and `Docs/Roadmap.md` for execution order.

The bounded Waterworks v1 non-visual validation is now green: the earlier
existing-save two-process gate passed 1/1 + 1/1 with zero runtime ERRORs
(commit `a6411c2745b5cf94316dd412a12eb61f3009a2fa`), and after the
one-cell-width rule the author reported `validate-source.ps1` PASS followed
by the current core `run-e2e.ps1` **7/7 PASS with zero runtime ERRORs**.
This does not establish visual acceptance or full user-modpack compatibility.
Subsequent feature expansion / Rice integration still waits for its roadmap slot.


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
**Status:** DONE for the non-visual v1 core — author-reported current static/build PASS and Pickle 7/7 + runtime ERROR 0

The source/graph/job/save vertical slice defined in `Docs/Design.md` is implemented. After the one-cell-width rule and the static-audit XML-inventory repair, the author reported `Scripts/validate-source.ps1` PASS and then `Scripts/run-e2e.ps1` PASS on the current source. That runner requires exactly **7/7** named scenarios, including the one-cell-width regression, and zero isolated runtime `[ERROR]` lines. The dedicated existing-save gate had separately passed **1/1 + 1/1** with zero runtime ERRORs in commit `a6411c2745b5cf94316dd412a12eb61f3009a2fa`.

This closes the non-visual core acceptance scope only. Wet/dry appearance, bridge occlusion and final canal art remain under the VIS workstream; full user-modpack compatibility and publication are not established.

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
**Status:** DONE for source/build/runtime checkpoint; visual acceptance remains separate

Current production source has author-reported static audit + RimWorld 1.6 C# build PASS and core Pickle **7/7** with runtime ERROR 0. This checkpoint does not approve the unaccepted renderer/art path or authorize publication.

### PROTO-WATERWORKS-API-001 — RimWorld 1.6 draw-style compatibility

**Owner:** Waterworks implementation
**Status:** DONE — source correction committed; build/runtime verification still required

- Audited RimWorld 1.6 `Verse.Designator`, `DesignatorManager` and `DrawStyle_Line`: `DraggableDimensions` is a removed/obsolete API.
- The dig/fill designators now expose the dedicated `AMJW_CanalLine` category with only Vanilla `Line` style; single-cell and cardinal straight drag are retained.
- A static contract check guards against reintroducing the old property.
- This is an **API-source audit**, not a successful compilation or in-game validation.

### TEST-WATERWORKS-STATIC-001 — repeatable source validation

**Owner:** Waterworks implementation  
**Status:** DONE — current source author-reported PASS

`Scripts/validate-source.ps1` passed after the stale XML-count assertion was replaced by the explicit 10-path inventory in commit `0ed279d699acd559bcabe9f864d99014513129e8`. This establishes static audit + RimWorld 1.6 production C# build for that current source. Runtime is tracked separately; visual shader/bridge appearance remains outside this gate.

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

**Owner:** Waterworks existing-save acceptance
**Status:** DONE — isolated two-process gate passed

The dedicated existing-save test passed in commit `a6411c2745b5cf94316dd412a12eb61f3009a2fa`: Phase A **1/1**, Phase B **1/1**, zero failed/skipped scenarios and zero runtime `[ERROR]` lines in both cold starts. It establishes the tested Vanilla-save -> newly installed Waterworks path for the isolated fixture, not general full-modpack compatibility. The later one-cell-width change did not change save schema; current core behavior is separately covered by the new 7/7 gate.

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

### VIS-WATERWORKS-001 — water/ditch/bridge appearance acceptance (2026-10-08)

**Owner:** Waterworks rendering/visual validation
**Status:** OPEN — source and E2E audit complete; actual rendered frames not yet evaluated

Read `Docs/GoldenPaths/VisualAcceptance.md` for the seven concrete visual acceptance checks and evidence policy. The current wet Def reuses Vanilla `WaterShallowRamp` + `Map/WaterDepth`, and dry Def reuses tinted Soil. Neither Def establishes the confirmed narrow-center-channel visual target, so do not presume it passes. Existing E2E verifies bridge topology rather than water-shader occlusion. Prefer a deterministic isolated rendered-map capture; do not use `-nographics`. Only ask for minimal human inspection if actual rendered capture cannot be automated safely. Do not create new canal art or expand gameplay before isolating the actual observed visual failure.

### VIS-WATERWORKS-002 — deterministic render fixture specification

**Owner:** Waterworks visual-test implementation
**Status:** OPEN — reproducible layout/capture contract documented; test code and rendered frames not yet created

`Docs/GoldenPaths/VisualFixture.md` defines a separate 17×17 scene (river mouth, wet trunk, T/cross, bend, isolated dry segment, two Vanilla bridges, Gravel reference) and capture/error/manifest gates. Keep the already green 6/6 core suite and 1+1 existing-save suite unchanged. Verify a real RimWorld 1.6 camera/screenshot/render API and compiler environment before implementing automated screenshots. No screenshot PASS or art acceptance is claimed.

### VIS-WATERWORKS-003 — isolated Pickle screenshot implementation (2026-10-08)

**Owner:** Waterworks visual-test implementation
**Status:** IMPLEMENTED IN SOURCE — Windows/RimWorld build and rendered-frame acceptance OPEN

Added `Tests/E2E/WaterworksVisualSteps.cs`, a separate `waterworks-visual.feature`, developer-only `VisualMod` metadata + harmless content marker, and `Scripts/run-visual-e2e.ps1`. The visual step creates a revealed 17x17 map scene with valid natural source, wet/dry canals, branches, two Vanilla bridge foundations and gravel reference. It requests three screen captures (connected/disconnected/restored), waits for real PNG files, validates header/dimensions, and writes a manifest to isolated `TestResults/Visual/SaveData/WaterworksVisual`. Runner demands the unique Pickle 1/1 PASS, runtime ERROR=0 and all screenshots present. `Tests/E2E/Steps.csproj` now references UnityEngine.ScreenCaptureModule and includes the visual step. Static audit checks that the visual runner is distinct from the existing core E2E. Normal ModsConfig and user saves remain untouched.

**Verification status:** no Windows RimWorld 1.6 compiler/runtime/render execution from the editing environment. The new test must be built first with `Scripts/run-visual-e2e.ps1`; if the screenshot API cannot render in the hidden process, diagnose the render surface rather than accepting empty/fake PNGs. Visual appearance PASS remains OPEN until actual frames are inspected; the accepted 6/6 + 1+1 suites are unchanged. No new art committed.

### VIS-WATERWORKS-004 — screenshots reveal one-state capture lag and full-cell channels

**Owner:** Waterworks visual-test implementation / rendering
**Status:** IN PROGRESS — capture timing/layout correction committed; rerun pending

Author-supplied `connected.png`, `disconnected.png`, `restored.png` and `manifest.txt` show real rendered output at 2560×1440, seed `AMJ-Waterworks-E2E`, camera center `(9,0,37)`. In visual review the supposedly connected frame lacks the laid-out canal, the disconnected frame displays wet channels, and the restored frame displays dry channels. This strongly suggests terrain mesh/screenshot timing lag (not proven by the existing Pickle state assertions). The captured water and dry cells also fill the full tile rather than a narrow channel; current Vanilla reuse does not meet `VIS-02`. The patch is framed near the map corner with extensive black/fog background. Bridge shader occlusion remains inconclusive.

`Tests/E2E/WaterworksVisualSteps.cs` now separates each terrain-state mutation from `CaptureScreenshot` with a 1500-ms settle interval, and selects the closest safe 17×17 patch to map center instead of the first match; `Tests/static_audit.py` guards both behaviors. These are **source fixes, not rerun evidence**. Next rerun only the isolated visual runner; inspect all three synchronized frames. If the capture still lags, replace the time wait with verified rendered-frame scheduling before altering production Waterworks code. Fix the full-cell appearance only after valid synchronized evidence. Do not mark visual gate DONE or commission art yet.

### VIS-WATERWORKS-005 — second 3-frame capture reviewed (2026-10-08)

**Owner:** Waterworks visual/render workstream
**Status:** IN PROGRESS — capture sequence visually synchronized; narrow-channel rendering still fails

Author supplied `connected(1).png`, `disconnected(1).png`, `restored(1).png` and `manifest(1).txt`. The connected capture visibly shows wet canal tiles, the disconnected capture shows dry tiles, and restored again shows wet tiles; the previous one-state screenshot lag is no longer evident. This supports the fixed settle interval for the current isolated scene, not a general GPU synchronization guarantee. Actual imagery remains full-cell-width water/tinted soil rather than a narrow excavated center channel with earth banks: `Docs/GoldenPaths/VisualAcceptance.md` VIS-02 remains FAIL, and dry ditch readability (VIS-01) needs improvement. Junctions are visible but their final seam appearance and the bridge/water occlusion cannot yet be accepted confidently at this zoom. The camera still favors the map corner; `manifest(1).txt` records center `(9,0,37)`, indicating that choosing the closest *valid* 17×17 patch did not guarantee centered framing.

Next: investigate minimal narrow channel rendering without changing network/terrain identities. Audit actual 1.6 terrain/material mesh and foundation ordering before selecting an overlay or textured solution; preserve bridge遮蔽 and river source boundary. Improve fixture by preparing a controlled central patch (without deleting non-test edifices) rather than merely preferring the nearest eligible map cell, and ideally capture a closer crop at fixed zoom. Keep all 7 subjective VIS gates open until new rendered proof. No new art or production rendering implementation accepted yet.

### VIS-WATERWORKS-006 — narrow rendering approach audit (2026-10-08)

**Owner:** Waterworks visual implementation
**Status:** IN PROGRESS — architectural trial order fixed, renderer source NOT yet implemented

The second synchronized screenshot set proves the current wet/dry TerrainDefs' appearance is full-cell and fails the required narrow excavated channel. Source audit confirms current TerrainDef fields only select a texture, edge blending, shader and precedence; no dedicated narrower geometry currently exists. General 1.6 terrain layering/bridge transparency examples suggest that relying on an alpha texture to reveal the original soil under a replaced terrain is unsafe without live proof. `Docs/Design.md` §8.0.1 now defines an ordered proof: (1) alpha/material feasibility in real terrain renderer, (2) if necessary a small cardinal-aware Waterworks-only rendering overlay, (3) Vanilla foundation occlusion, (4) minimal art only if unavoidable. Do not replace the connected-component state machine, modify saved terrain identity, add generic Harmony mesh patches, or claim rendering implementation complete. Next gate: controlled in-game material/mesh prototype screenshots of one straight canal and bridge, then all junction forms.

### VIS-WATERWORKS-007 — cardinal narrow-channel geometry source (2026-10-08)

**Owner:** Waterworks visual renderer
**Status:** IN PROGRESS — geometry model committed; actual draw submission NOT implemented or compiled

Added `Source/CanalVisualTopology.cs` containing a rendering-only 4-bit N/E/S/W connection mask and center/arm rectangles for all 16 cardinal configurations (current prototype half-channel=0.20 cell, half-bank=0.34 cell). Dry and wet TerrainDefs remain authoritative and unchanged. `Tests/static_audit.py` now checks the new geometry contract and all 16 mask edge exits; `Tests/E2E/WaterworksVisualSteps.cs` checks line, elbow and cross masks on the real generated test map. This is useful geometry data for a future canal-specific mesh/layer, **not a visible narrow-water renderer yet**. No renderer draw call, texture, player-save change or bridge occlusion claim has been made.

Next: build the production DLL and visual E2E against actual 1.6 binaries, correct compile/API issues if any, and prototype a real terrain-lifecycle-aware section layer below Vanilla bridge foundations before accepting screenshots. Do not change pathing, source graph or saves. Existing 6/6 and 1+1 acceptance remain undisturbed.

### VIS-WATERWORKS-008 — Unity mesh construction proof stage (2026-10-08)

**Owner:** Waterworks visual implementation
**Status:** SOURCE READY / RUNTIME UNVERIFIED — section-layer draw integration still OPEN

Added `Source/CanalVisualMesh.cs`: pure, no-state-write Unity Mesh builder using a disjoint 3×3 center/arm subdivision for all 16 cardinal connection masks, world-aligned UVs, and configurable half-channel width. It does not draw over the map, alter TerrainDefs, add Harmony, or modify graph/persistence. Extended the isolated Pickle visual scenario to build all 16 meshes on the game thread and verify expected vertex/triangle counts, disposing temporary mesh objects afterward. Drawing these meshes *under the Vanilla bridge foundation* requires a separate RimWorld 1.6 SectionLayer integration audit and rendered-frame verification. Current full-cell appearance is unchanged.

**Next:** run `Scripts/run-visual-e2e.ps1` against the Windows RimWorld 1.6 DLLs; investigate any compile/runtime failure before additional source writes. Following a green test, prototype section-layer material/draw integration, then validate bridge occlusion and narrow banks in new screenshots. Do not mark VIS-02 or VIS-05 passed yet.

### VIS-WATERWORKS-009 — third screenshot set / mesh generation validation (2026-10-08)

**Owner:** Waterworks visual implementation
**Status:** IN PROGRESS — captured wet/dry transitions; mesh draw integration pending

Author submitted `connected(2).png`, `disconnected(2).png`, `restored(2).png` and `manifest(2).txt` from another visual E2E execution. Wet, dry and wet rendering states are visible in the intended order. The width/soil-bank presentation is unchanged and remains full-cell; this is expected because `CanalVisualMesh.Build` is constructed and checked by Pickle, not submitted to the live renderer. The screenshots alone do not establish the mesh count assertion result or ERROR=0 (summary and Player.log were not provided). Continue with actual SectionLayer/lifecycle and bridge ordering integration, keeping game-state and saves unchanged. Do not claim narrow renderer passed.

### VIS-WATERWORKS-010 — loaded RimWorld render API evidence gate (2026-10-08)

**Owner:** Waterworks renderer
**Status:** TEST IMPLEMENTED — actual loaded-game API report pending

Added isolated `Tests/E2E/WaterworksRenderProbe.cs` and integrated it into `WaterworksVisualSteps.cs`, `Steps.csproj`, and `Scripts/run-visual-e2e.ps1`. The visual E2E now writes `render-api.txt` alongside the three PNGs/manifest in `TestResults/Visual/SaveData/WaterworksVisual`, reporting loaded constructors, fields, properties, and methods on SectionLayer, Section, MapDrawer, SectionLayer_Terrain and TerrainGrid. This is a diagnostic-only reflection probe; no production Harmony patch or unverified draw method has been added. Static audit checks the report contract.

**Decision boundary:** do not commit a speculative SectionLayer integration until its actual RimWorld 1.6 constructor/hook/ordering APIs are established. User-side next step is one isolated visual test run and providing `render-api.txt` with any failed summary/log; the current visuals remain unchanged. After obtaining the report, implement a narrow-channel layer and validate the underlying terrain suppression and Vanilla bridge occlusion. Existing 6/6 and existing-save 1+1 tests remain unchanged.

### VIS-WATERWORKS-011 — 1.6 SectionLayer runtime signatures obtained (2026-10-08)

**Owner:** Waterworks renderer
**Status:** IN PROGRESS — API evidence received; active section-layer ordering report pending

Author provided `render-api.txt` from the loaded RimWorld 1.6. Observed: `Verse.SectionLayer(Section)` constructor; `Verse.Section.GetLayer(Type)`, `RegenerateSingleLayer(SectionLayer)`, `RegenerateDirtyLayers`, `DrawSection`; `Verse.MapDrawer.SectionAt(IntVec3)`, `MapMeshDirty(IntVec3, UInt64[, Boolean, Boolean])`, `RegenerateLayerNow(Type)`, `DrawMapMesh`; `SectionLayer_Terrain.Regenerate()` and `TerrainGrid.SetFoundation/SetTerrain/UnderTerrainAt`. These names establish surface signatures, **not** registration/sort order, callbacks to add third-party layers, or the correct suppression of full-cell water terrain under a narrower visual layer.

Extended the test-only render probe to dump `MapDrawLayer` and `SectionLayer_Dynamic` declared members plus the actually instantiated `Section.layers` ordered type list at the visual fixture position, using reflection strictly for diagnostics. The existing test now emits these in `render-api.txt` and static audit checks the report wiring. No speculative global Harmony patch or production section layer injection has been made, and gameplay TerrainDefs/connectivity remain untouched. Next required external evidence is the single rerun of the isolated visual E2E; the updated `render-api.txt` should include `RUNTIME SECTION LAYER ORDER` with ordered indices. After reviewing that, implement a minimal rendering integration preserving Vanilla foundation occlusion and verify screenshots. This gate is not yet accepted.

### VIS-WATERWORKS-012 — actual Section list received; draw API shape probe extended (2026-10-08)

**Owner:** Waterworks render integration
**Status:** IN PROGRESS — game build and one focused render API diagnostic pending

The author's `render-api(1).txt` shows a RimWorld 1.6 map with 23 SectionLayers. Notable instances are `Verse.SectionLayer_Terrain` (index 13), `Verse.SectionLayer_Watergen` (16), `RimWorld.SectionLayer_BridgeProps` (18), and `RimWorld.SectionLayer_TerrainEdges` (21). The list is the internal Section.layers collection, **not evidence of compositing order**. Loaded `MapDrawLayer` exposes `DrawLayer`, `GetSubMesh(Material)`, `FinalizeMesh(MeshParts)`, `CreateFreeSubMesh(Material, Map)`, and `Regenerate`. These public/private signatures do not establish which members are virtual and how to emit the narrow mesh without full-cell terrain showing underneath.

The test-only `WaterworksRenderProbe` now records whether draw/regenerate methods are virtual and inspects `LayerSubMesh`, `SectionLayer_Watergen`, `SectionLayer_BridgeProps` in addition to the already captured types. This is diagnostic-only and preserves the accepted gameplay pipeline; no speculative production section layer was committed. Static contract checks were extended. The next user-side artifact needed is the updated isolated `render-api.txt` or a compile error log. Then implement the smallest renderer appropriate to the proven hook, maintaining bridge occlusion and no graph/save changes.

Do not mistake the ordered reflection list for render order; VIS-02 and VIS-05 remain not accepted.

### VIS-WATERWORKS-013 — fix CS0122 on internal SectionLayer_Watergen (2026-10-08)

**Owner:** Waterworks visual E2E renderer API diagnostics
**Status:** SOURCE FIX COMMITTED — Windows E2E rerun pending

The author attempted the extended rendering probe and `Tests/E2E/Steps.csproj` failed before RimWorld launch: `CS0122: SectionLayer_Watergen is inaccessible due to its protection level` in `WaterworksRenderProbe.cs`. This is a test-only compile error, **not** a production Waterworks error or evidence of a failed visual mesh. Updated the probe to look up internal `Verse.SectionLayer_Watergen` and `RimWorld.SectionLayer_BridgeProps` with `typeof(SectionLayer).Assembly.GetType(fullName, false)` and report unavailable names gracefully; no direct compile-time references to either concrete type remain. Updated `Tests/static_audit.py` to reject reintroduction of those inaccessible `typeof` calls and require the runtime-name probes. Code-only consistency checks were performed; no Windows game DLL/SDK is available in the editing environment to rerun the real build. The separate 6/6 and existing-save 1+1 acceptance records are unchanged.

**Next:** run the isolated visual E2E again to test the previously blocked compile and receive `render-api.txt` with actual method virtual flags and bridge/water layer signatures. If another compile error occurs, fix that precise API issue instead of introducing speculative production draw-layer patches. Full-cell appearance remains until a renderer is attached.

### VIS-WATERWORKS-014 — native canal SectionLayer prototype (2026-10-08)

**Owner:** Waterworks rendering
**Status:** IMPLEMENTED IN SOURCE — Windows compilation / images / bridge occlusion acceptance OPEN

The author's `render-api(2).txt` confirmed virtual `MapDrawLayer.DrawLayer` and `Regenerate`, `LayerSubMesh` mutable geometry buffers, `SectionLayer_Watergen` and `SectionLayer_BridgeProps` loaded signatures, and 23 existing layers. Inspection of RimWorld 1.6's `Section` implementation confirms concrete `SectionLayer` subclasses are discovered by type enumeration. The layer list order alone does not prove compositing order; bridge-underlay safety is handled by explicitly suppressing canal overlay on foundation cells.

Added `Source/SectionLayer_AMJW_Canal.cs` as native rendering-only layer, `CanalVisualMesh.Append` to add disjoint quads to persistent submeshes, and adjacent-section terrain-mesh invalidation on canal topology changes. The wet/dry TerrainDefs still carry gameplay state, but both draw neutral soil under the narrower bank and narrow surface; removed whole-tile `Map/WaterDepth` so it cannot obscure the canal's width. No additional texture assets, Harmony patch, or save-format change. Visual E2E now requires `Section.GetLayer(typeof(SectionLayer_AMJW_Canal))` and nonempty finalized mesh. `Docs/Design.md` §8.0.2 records limitations, including temporarily static water and bank colour differing from original ground.

Next external gate: `Scripts/run-visual-e2e.ps1` must compile the new production SectionLayer and pass isolated Pickle 1/1 with ERROR=0, then compare connected/disconnected/restored screenshots for water width, bridge coverage, junction seams and supply-mouth rendering. If build fails, inspect the compiler error instead of stack-pushing commits. Existing 6/6 and existing-save 1+1 suites unchanged; their prior acceptance does not automatically cover new rendering changes.

### VIS-WATERWORKS-014-STATIC — obsolete texture assertions corrected (2026-10-08)

**Owner:** Waterworks validation
**Status:** FIXED IN SOURCE — runtime build pending

Preflight review immediately after the initial native SectionLayer prototype found that `Tests/static_audit.py` still asserted the old wet `WaterShallowRamp` TerrainDef and `Map/WaterDepth` shader, which necessarily contradict the new Soil-underlay/SectionLayer design. The static audit now requires both gameplay TerrainDefs to use neutral Soil, equal tint, FadeRough edges and no full-cell water-depth shader; it confirms that the wet texture is instead referenced in `Source/SectionLayer_AMJW_Canal.cs`. No changes to Waterworks graph, saved data or bridge rules. Actual Windows compile and scene appearance remain OPEN.

### VIS-WATERWORKS-015 — validator auto-detects local RimWorld install (2026-10-08)

**Owner:** Waterworks test tooling
**Status:** SOURCE FIX COMMITTED — Windows build pending

Author ran `Scripts/validate-source.ps1` unparameterized from `.../RimWorld/Mods/AncientMedievalJapanWaterWorks`. Static audit passed; no C# build ran because the script default only used `RIMWORLD_DIR`. Updated `Scripts/validate-source.ps1` to infer `<RimWorld>/Mods/<mod>` relative to the script location only when both explicit `-RimWorldDir` and environment variable are absent. Require the actual game `Assembly-CSharp.dll` to be present before accepting the inferred path. Preserve parameter/environment precedence and explicit failure for other layouts. Updated `Tests/static_audit.py` to guard the fallback contract. No gameplay, Def, save, or visuals were changed; Windows compilation remains unverified. `run-visual-e2e.ps1` already supplies the explicit parameter.

### ADD-CHANGENOTE-20261008 — Versioned Workshop update notes

**Owner:** Ancient-Medieval-Japan-Waterworks packaging/release
**Status:** SOURCE IMPLEMENTED — dedicated metadata CI pending; gameplay/release gates unchanged

Project `Docs/WorkshopChangenotes.md` now applies here: `About/Manifest.xml`, `About/Changelog.txt` and `About.xml` agree on `0.1.0-dev`. A narrow new workflow checks the metadata and YADA retention without running unowned gameplay tests. These subscriber metadata files have no effect on gameplay, packageId or Mod dependency rules. No Steam publishing or new runtime verification occurred.

### VIS-WATERWORKS-016 — Medieval Overhaul 壕 art-first reference verified (2026-10-08)

**Owner:** Waterworks visual design
**Status:** RESEARCH COMPLETE; SIDE-BY-SIDE DRY/WET IMAGE PROPOSAL PENDING; render-code trial loop PAUSED

User suggested basing the canal depiction on MO's actual dry-earth `壕` instead of continuing unproductive implementation/screenshot iterations. Inspected supplied 1.6 Medieval Overhaul archive `3219596926.zip` and Japanese translation archive `2665554648.zip`. Active MO building `DankPyon_Trench` references `Things/Building/Linked/Trench_Atlas` (four-by-four 320×320 earth ditch texture), `Graphic_Single`, `linkType Basic`, `Custom5`, `FloorEmplacement`; reinforced `DankPyon_RTrench` is stone-lined and not a standard agrarian canal reference. The apparent `DankPyon_TrenchTerrain` is commented out. These are visual precedents, not reusable game-state mechanics or permission to redistribute MO artwork.

Formal design now records the MO earth-ditch visual basis, dry and wet should share banks and differ at the bed; `Docs/GoldenPaths/VisualAcceptance.md` corrects stale pre-prototype TerrainDef assertions and adds an art-approval gate. Stop speculative renderQueue/width fixes and repeated user E2E requests until a demonstrable side-by-side visual target is accepted. No production texture or C# change occurred in this action; no direct copying of MO textures into the Mod.

### VIS-WATERWORKS-017 — author's sketches converted to 16-mask upper-view contour specification (2026-10-08)

**Owner:** Waterworks visuals
**Status:** TEXT DESIGN RECORDED; user-facing concept sheet/pixel-art approval OPEN; renderer modifications and repeated E2E PAUSED

Read 3 sketches supplied in the Waterworks design discussion: horizontal dry ditch with sloping soil shoulders and recessed dark bed, same wet ditch with water retained only inside bed, and a top-down vertical plus four-way intersection study. Recorded formal text specification in `Docs/Design.md` §8.0.4 for 16 cardinal variants (isolated 1 / terminal 4 / straight 2 / elbow 4 / T 4 / cross 1). Wet/dry share cut banks; only recessed bed contents differ. Junctions use a gently expanded continuous concavity, closed edge retains soil; neighbor boundaries must align; bridge and source-mouth compatibility retained without new game mechanics. The sketches' tile lines are reference guides, not art. MO trench is an appearance reference only, not redistributable art or gameplay code. Updated `Docs/GoldenPaths/VisualAcceptance.md` with an image-review checklist and reinforced art-first approval. Existing `CanalVisualTopology.Rectangles` and `SectionLayer_AMJW_Canal` hard-rectangle values remain **unaccepted prototype code**, not finalized art measurements.

**Next:** prepare a single coherent side-by-side top-down *visual proposal*, dry/wet and straight/elbow/T/cross at common scale, for the author's evaluation. No E2E rerun or arbitrary code/material width changes before visual approval. Once an image is approved, match art/material/mesh/bridge approach to that reference; then perform one focused automated/runtime validation pass. No production code, Def, texture, save or gameplay change in this documentation pass.

### VIS-WATERWORKS-018 — choose substrate-adaptive grayscale trench material (2026-10-08)

**Owner:** Waterworks visual design
**Status:** ARCHITECTURE DECIDED — art and engine compositing verification OPEN; no graphics implementation yet

The author asked to choose the best method for the same hand-drawn ditch appearance on differently colored terrains. Formal `Docs/Design.md` §8.0.5 selects a reconstructed **original TerrainDef material/texture** as ground, a shared transparent monochrome alpha shadow/highlight mask for the excavated banks and dry bed, and an independently clipped wet-only water layer. Saving DefName already supports material lookup but does not render the original terrain underneath an opaque canal TerrainDef, nor preserve an old ColorDef tint. The renderer must explicitly redraw it and verify draw order/material depth. Neither single-color per-terrain tint nor separate atlases for every soil is default. Keep original wet/dry, source connectivity, bridge and fill semantics. Update §6.3 to say original terrain metadata has both fill-restoration and appearance uses without becoming a general terrain history.

`Docs/GoldenPaths/VisualAcceptance.md` adds VIS-08 for matching actual Soil/Gravel/RichSoil ground appearance; `Docs/GoldenPaths/VisualFixture.md` adds a **separate future** color-adaptive visual comparison without altering existing accepted fixtures. The previously planned image sheet remains the next step before modifying rendering code; produce top-down wet/dry variants of the same banks and sample original ground texture contexts. No new image asset, C# file, XML Def or save schema in this change; runtime feasibility still unverified. Stop repeated game tests until art approved and a coherent focused compositing test is ready.

### VIS-WATERWORKS-019 — shared-mask PNG artwork v2, mechanically checked, visual approval OPEN (2026-10-08)

**Owner:** Waterworks texture pipeline
**Status:** CANDIDATE CREATED LOCALLY, NOT APPROVED OR COMMITTED TO PRODUCTION TEXTURES; all runtime gates OPEN

The author's explicit objection to three independently generated atlases was correct: duplicate/missing directional variants and bed-water mismatches made them unusable. Created a local deterministic source generator and ZIP `Waterworks_connected_assets_v2_NOT_APPROVED.zip`, delivered in the conversation, containing **16 N1/E2/S4/W8 masks**, each with `Shadow`, `Highlight`, `Water` and `BedSupport` RGBA/grayscale source layers, corresponding 80x80 production-size derivatives, and 320x320 row-major atlases. All visual layers derive from **one signed contour function per mask**; the water interior cannot be independently reshaped by ImageGen. 720 numeric QA assertions passed, checking the 16 directional openings, bed containment, contiguous water, within-edge mask equivalence and every possible N/S and E/W paired border. Generated previews composite actual 80px output over **simulated**, not verified Vanilla, soil backgrounds. No MO art pixels are shipped.

**Limitations:** despite passing numerical tests, the art still tends toward smooth regular contour bands and has **NOT** received author approval as convincing hand-dug earth. This is a *development candidate*, not production-ready game content. The current `Source/SectionLayer_AMJW_Canal.cs` still uses its old hard rectangles / vanilla materials: no new PNGs are wired to the renderer, no RimWorld DLL compilation or in-game tests have been run, and no local candidate binary art or generator code was committed to GitHub in this task. Avoid falsely claiming release readiness. Next stage is visual-material review/targeted improvements, then durable accepted master storage in `Art/Sources/` and integration under `Textures/`, followed by one focused loaded-game render test. User should not be made responsible for checking trivial mask correctness.

### RULE-AUDIT-20261008 — operating-rule consolidation

**Owner:** Project common rules; this repository retains its local specification and gates.
**Status:** SOURCE RESTRUCTURED; validation/publication evidence is recorded in Project `Docs/RuleAudit.md` and actual commit/CI results, not inferred here.

AGENTS now routes through Project `Docs/SharedRules.md` stop conditions and task procedures. New development requires VE and non-VE source/evidence comparison plus a justified implementation decision. Static/runtime/specification/distribution/publication remain separate states. Historical records below/above retain their original scope; this entry does not reopen paused work, change gameplay/dependencies/art/versions, or supersede owner runtime/release blockers. Main-only Coordination means one authoritative integrated log, not deleting branch snapshots. No Steam/2game update is claimed.

### VIS-WATERWORKS-020 — cross-chat connected tile workflow retained in canonical sources (2026-10-08)

**Owner:** Waterworks art/production
**Status:** DOCUMENTATION LINKAGE DONE; imagery and renderer approval still OPEN

The author pointed out that a previous chat (“マップチップ生成のコツ”) cannot reliably be read in full from a new task/chat, and shared chat URLs are not a dependable canonical technical reference. AMJ Project now documents the reusable connected-map-tile production procedure in `Docs/GoldenPaths/TextureAssetPipeline.md` under “Connected map tiles — reusable workflow” (Project commit `ef444d6a2257b1482dbfba1135ade2583545f290`). The Waterworks formal design `Docs/Design.md` §8 now links this path as a startup production route. The link/decision is a technical *distillation of known prior guidance*, not a claim that the original chat's entire transcript was recovered. Previous stage `VIS-WATERWORKS-019` remains the status authority for the unapproved candidate ZIP, pending artistic approval and renderer integration. No gameplay, texture, original-ground rendering, build or loaded-game validation was performed for this documentation handoff.

### VIS-WATERWORKS-021 — deterministic EW candidate gate (2026-10-09)

**Owner:** Waterworks art/production tooling  
**Status:** TOOLING IMPLEMENTED; MECHANICAL LOCAL TEST PASS; visual approval and production assets remain OPEN

Added a repository-owned candidate pipeline so the connected-tile rule is no longer prose-only. `Scripts/build-canal-tile.ps1 -Mask EW` routes to a deterministic Python builder that fixes the EW connection geometry, derives Dry/Wet from the same shadow/highlight/bed layers, clips water to the bed, validates exact E/W edge equality and PNG integrity, and writes only to `TestResults/CanalTiles/EW` after all checks pass. Unsupported masks and `-All` are intentionally rejected until the EW visual target is explicitly approved. `Tests/test_canal_tile_pipeline.py` also verifies deterministic SHA-256 reproduction and both rejection paths.

Local implementation check: the first candidate run correctly failed its mechanical gate because antialiasing left mismatched edge/containment pixels; the generator was corrected and the regression test then passed. This is evidence that the stop gate actually blocks bad output rather than merely documenting a preference.

The current generated water/relief is debug material only and is not accepted art. No candidate PNG, `Art/Sources` master, production `Textures`, renderer source, gameplay code, save data, Workshop payload or Steam metadata is changed by this tooling commit. `VIS-WATERWORKS-019/020` remain the authority for prior unapproved art and the shared connected-tile procedure; `Docs/Design.md` §8 and `Docs/GoldenPaths/VisualAcceptance.md` remain the visual acceptance authority.

### VIS-WATERWORKS-022 — accepted 16-mask relief integrated and captured (2026-10-09)

**Owner:** Waterworks rendering
**Status:** LOCAL IMPLEMENTATION / RENDERED TEST PASS; author in-game visual review and publication remain separate.

Author selected the original 16-mask dry set (not the narrowed experiment), and requested existing Core water beneath common relief with no wet PNGs. Exact masters/derivatives now live under Art/Sources and Textures/Terrain/AMJW/Canal. Design section 8.0.6 and Docs/GoldenPaths/CanalOverlayRendering.md are the current source of truth. Earlier VIS-001..021 iteration records are superseded by those sources and retained in Git history; the old EW debug generator is not the selected runtime asset path.

SectionLayer redraws stored original terrain, clips Core WaterShallowRamp to generated bed geometry, then overlays the accepted transparent relief. A detected opaque-base draw-order failure was repaired before final captures. The final private-desktop visual run TestResults/Visual/20261009-102859 passed 1/1 with runtime ERROR=0, producing connected/disconnected/restored and all16-dry captures on real terrain. Core regression passed 7/7 with runtime ERROR=0 including construction, 2x2 exclusion and save/reload. Asset checks passed exact hashes, PNG integrity and 256 compatible border comparisons. Tests use separate profiles; no normal save/config edits.

The initial capture above used a simplified water material; VIS-023 supersedes its wet rendering. Historical per-cell tint/pollution reproduction and Workshop publication are not claimed. Current work is on codex/canal-overlay-render; no remote push/merge is implied by this local record.

### VIS-WATERWORKS-023 — native water surface without dry floor shadow (2026-10-09)

**Owner:** Waterworks rendering
**Status:** LOCAL IMPLEMENTATION / RENDERED TEST PASS.

Wet canals now use WaterMovingShallow's native TerrainWater material and waterDepthMaterial on the WaterDepth subcamera layer. The accepted relief is restricted to the complement of the bed while wet, so its dark dry floor cannot tint the water. Dry canals retain the complete accepted image; all 16 PNG hashes remain unchanged and no wet images were added. Generated rectangles merge identical spans without changing covered pixels; the largest geometry has 28 quads, below the vertex limit even across a full 17x17 section.

Final private-desktop run TestResults/Visual/20261009-175524 passed 1/1 with runtime ERROR=0. The real-game test checks native surface/depth meshes during supply, disconnection and restoration, and records connected.png, disconnected.png, restored.png and all16-dry.png under SaveData/WaterworksVisual. Static checks verify complementary coverage with no dry shadow over water, unchanged PNGs and all 256 edge pairs. The earlier 7/7 core regression belongs to VIS-022; it was not rerun for this rendering-only correction. Normal user saves/configuration were preserved.

Narrow-channel depth filtering can still differ visually from a broad source; exact RGB matching and animation behavior are not established by these still captures. No remote push, merge or publication occurred.

### VIS-WATERWORKS-024 — plain board on Vanilla bridges over canals (2026-10-09)

**Owner:** Waterworks bridge visual-only integration
**Status:** IN PROGRESS — PR #7 OPEN, source prepared; Windows build, actual Pickle, screenshots and author appearance acceptance PENDING

The author's loaded-game screenshots show a plain deck over the E/W canal but a visibly different additional hanging plank/support under the N/S canal bridge. The author specified retaining the ordinary **Bridge** build action, not adding any canal cover item, changing pathing/support, or creating/rotating assets: use the same plain board appearance at every connection shape and direction.

PR [#7](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Waterworks/pull/7) implements a small scoped Harmony postfix against RimWorld 1.6's `SectionLayer_BridgeProps.ShouldDrawPropsBelow`: only a standard `TerrainDefOf.Bridge` foundation on AMJW wet/dry canal suppresses the additional hanging/southern props quad. The Vanilla bridge top graphic, all gameplay/graph/save mechanics and non-canal bridges are unchanged. Direct graphics-only mechanism is needed because the Vanilla props predicate provides no per-cell Def override. This introduces the standard Harmony Mod as a dependency and uses compile-only Lib.Harmony.Ref; no Harmony DLL is bundled. Formal truth: `Docs/Design.md` §8.0.7 and `Docs/GoldenPaths/CanalOverlayRendering.md`.

PR-local E2E additions exercise Vanilla's patched predicate for horizontal and vertical canal bridges (expect no extra props) and a temporary non-canal bridge control (expect ordinary props). Metadata CI passed for PR head `da3644c4c5df4b95ed70cff4301ac6964c21799a`. **Do not merge PR #7 or claim a loaded-game pass yet**: Windows `Scripts/validate-source.ps1`, `Scripts/run-visual-isolated.ps1` Pickle 1/1, runtime ERROR=0 and comparison of connected/disconnected screenshots against the author's visual intent must be checked on that branch. Earlier 7/7 and visual 1/1 are evidence only for prior main. Steam publication remains on hold.

### VIS-WATERWORKS-025 — bridge plain-board fix merged and visually accepted (2026-10-09)

**Owner:** Waterworks visual integration
**Status:** PR #7 MERGED / WINDOWS BUILD PASS / ISOLATED VISUAL E2E PASS / AUTHOR VISUAL ACCEPTANCE CONFIRMED; Workshop/public release still HOLD

Supersedes the pending status in VIS-WATERWORKS-024. The author tested PR #7 head `da3644c4c5df4b95ed70cff4301ac6964c21799a` on the local RimWorld 1.6 installation. `Scripts/validate-source.ps1` reported asset/static gates PASS and C# build **0 warnings / 0 errors**. `Scripts/run-visual-isolated.ps1` exited **0**. That runner hard-requires actual Pickle **1/1**, **zero runtime [ERROR] entries**, and all four screenshots; its visual scenario tests patched bridge-prop suppression on both horizontal and vertical Waterworks bridges, plus preservation of the predicate for an ordinary non-canal Vanilla bridge. The author posted supplied/dry frames and reported the result visually acceptable; the previous under-plank beneath the vertical bridge is absent, while both bridge board tops look consistent. No image rotation, new graphics, additional construction item, or water-network change is needed.

PR [#7](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Waterworks/pull/7) merged via commit `0da1a656d76be46ae9a831d672d7ae333ee2ff44`; its triggered Add Changenote metadata CI passed. The formal acceptance procedure `Docs/GoldenPaths/VisualAcceptance.md` marks **VIS-05 only** PASS; other visual criteria are not implicitly approved. The Bridge graphics change introduces a mandatory standard Harmony runtime Mod, as documented in `About/About.xml` and `Docs/Design.md` §8.0.7. Core and add-to-existing-save gates from earlier code remain separate evidence; broad modpack compatibility, Steam upload and public release are not claimed.
