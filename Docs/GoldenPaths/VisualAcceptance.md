# Waterworks v1 visual acceptance

This is the concrete rendering acceptance procedure for the `Docs/Design.md` §7–8 baseline. It is **not** evidence that the current Vanilla asset reuse already passes. Do not create replacement art before evaluating the live render path.

## Current source baseline (not visually accepted)

- `AMJW_DugCanalWet`: `Terrain/Surfaces/WaterShallowRamp`, `Map/WaterDepth`, `edgeType=Water`, `renderPrecedence=389`.
- `AMJW_DugCanalDry`: `Terrain/Surfaces/Soil`, tint `(0.72, 0.64, 0.54)`, `edgeType=FadeRough`, `renderPrecedence=388`.
- Both occupy an entire terrain cell. Neither Def presently implements a narrower central ditch shape. The required visually narrow watercourse therefore remains **UNVERIFIED**, and must not be inferred from the connectivity E2E.
- `Bridgeable` permits a Vanilla bridge foundation and the existing E2E verifies graph/terrain preservation; it does **not** assert water shader occlusion under the bridge.

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
5. If VIS-02 fails, first test whether Vanilla materials/edge behavior can be constrained to a visibly narrow channel without changing the graph or source semantics. Otherwise design only the minimum bank/rim overlay; keep new art postponed until the specific failure is understood.
6. If VIS-05 fails, inspect RimWorld's foundation/terrain render ordering and shader flags before altering network mechanics or inventing a custom bridge.

## Boundaries

No new water source, canal consumer, bridge family, slope simulation or gameplay feature belongs in this visual gate. `Docs/Design.md` remains authoritative for visual scale and rendering semantics. Passing this gate does not establish full external modpack compatibility.
