# Waterworks v1 visual acceptance

This is the concrete rendering acceptance procedure for the `Docs/Design.md` §7–8 baseline. It is **not** evidence that the current Vanilla asset reuse already passes. The original full-cell and narrow-material trial frames have been evaluated. Approve a concrete visual target before resuming renderer iterations or new production artwork.

## Current source baseline (not visually accepted)

- **Legacy rejected visuals:** wet `WaterShallowRamp` + `Map/WaterDepth` and dry tinted soil occupied full cells; subsequent Waterworks-only narrow-section-layer screenshots did not show a legible channel. These captures do **not** meet VIS-01/VIS-02.
- **Current code prototype:** both `AMJW_DugCanalWet` and `AMJW_DugCanalDry` retain distinct gameplay TerrainDefs but use Soil as the full-cell underlay. `SectionLayer_AMJW_Canal` attempts a narrower bank (0.68 cell) and water/earth bed (0.40 cell), via `CanalVisualMesh.Append`; final visibility/material queues and foundations remain unaccepted.
- **Next art baseline:** see `Docs/Design.md` §8.0.3: MO 1.6's `DankPyon_Trench` **壕** and its earth-cut connected atlas are a *verified visual reference*, not a dependency or approved copied asset. Wet and dry should share the same sculpted banks.
- `Bridgeable` permits a Vanilla foundation and the connectivity E2E validates state preservation; screenshots, not a Def assertion, must establish bridge occlusion.

**Visual approval gate before another round of renderer changes:** inspect and agree on a dry/wet side-by-side proposed look and connected variants, based on the verified MO reference. Do not run repeated full rendered E2E trials to compensate for an undecided visual target.

## Minimum reproducible visual scene

Use an isolated map with Waterworks and its supported Vanilla dependencies only. Do not edit the user's active ModsConfig or saves. Use the normal rendering path (never `-nographics`).

Prepare a straight supplied canal from a natural river, a 90-degree bend, a T-junction and a cross-junction, a disconnected dry segment, and Vanilla bridge foundations over (a) a straight canal cell and (b) a junction-adjacent cell. Include ordinary soil and gravel next to the canal for scale and blending comparison. Keep both wet and dry cells visible in the same view and use an identical zoom level for comparable images.

## Acceptance matrix

| ID | Automated state prerequisite | Visual acceptance | Outcome |
| --- | --- | --- | --- |
| VIS-01 | wet/dry Defs load; supplied terrain switched | Wet and dry are unambiguous in normal map view, without debug overlays | OPEN |
| VIS-02 | cardinal adjacency accepted, diagonals rejected | A one-cell terrain corridor appears as a **noticeably narrower excavated channel**, with surrounding banks; no full-cell river ribbon | OPEN |
| VIS-03 | T/cross connectivity accepted | Straight, bend, T and cross shapes show no missing center, seam, broken corners or discontinuous water animation | OPEN |
| VIS-04 | valid freshwater neighbor supplies canal | River/pond mouth has no misleading dry gap or abrupt water-edge artifact | OPEN |
| VIS-05 | bridge foundation preserves connectivity | Bridge planks visually cover water; water does not draw above bridge, shimmer through it or spill beyond adjacent tiles | OPEN |
| VIS-06 | source removed / reconnected | Rebuilt wet↔dry state changes are visible without ghost water, stale mesh or retained wet shader effects | OPEN |
| VIS-07 | Gravel/Soil restoration E2E passed | Filled terrain visually matches surrounding ground; no leftover rim/water artifact | OPEN |

## Execution and evidence

1. Reuse the existing 6/6 graph/pawn/save-load E2E and 1+1 existing-save E2E as logic gates; do not rerun them solely to substitute for visual inspection.
2. Prefer a deterministic Pickle/Quickstarts fixture that prepares the above scene and captures **actual rendered frames** into isolated `TestResults` (when a verified capture API is available). Keep render enabled and require zero `[ERROR]` messages. A terrain-Def assertion or screenshot of an off-screen/unrendered map is not visual evidence.
3. Until an actual capture path is verified, a single in-game scene and screenshot set is sufficient for the subjective parts; do not demand repeated manual campaigns. Record the version/commit, Mod list, map zoom and whether water animation was observed.
4. For each VIS item mark PASS / FAIL with image evidence. Do not label all rendering PASS just because E2E passes.
5. VIS-02 has already failed for full-cell materials and remained illegible in the initial thin-mesh capture. Compare a clear dry/wet reference concept based on MO's excavated earth ditch (§8.0.3), approve its geometry/art first, and only then resume focused implementation. Do not re-run the same visual gate with arbitrary width/color guesses.
6. If VIS-05 fails, inspect RimWorld's foundation/terrain render ordering and shader flags before altering network mechanics or inventing a custom bridge.

## Boundaries

No new water source, canal consumer, bridge family, slope simulation or gameplay feature belongs in this visual gate. `Docs/Design.md` remains authoritative for visual scale and rendering semantics. Passing this gate does not establish full external modpack compatibility.
