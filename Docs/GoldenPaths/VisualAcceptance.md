# Waterworks v1 visual acceptance

This is the concrete rendering acceptance procedure for the `Docs/Design.md` §7–8 baseline. It is **not** evidence that the current Vanilla asset reuse already passes. The original full-cell and narrow-material trial frames have been evaluated. Approve a concrete visual target before resuming renderer iterations or new production artwork.

## Current source baseline (not visually accepted)

- **Legacy rejected visuals:** wet `WaterShallowRamp` + `Map/WaterDepth` and dry tinted soil occupied full cells; subsequent Waterworks-only narrow-section-layer screenshots did not show a legible channel. These captures do **not** meet VIS-01/VIS-02.
- **Current code prototype:** both `AMJW_DugCanalWet` and `AMJW_DugCanalDry` retain distinct gameplay TerrainDefs but use Soil as the full-cell underlay. `SectionLayer_AMJW_Canal` attempts a narrower bank (0.68 cell) and water/earth bed (0.40 cell), via `CanalVisualMesh.Append`; final visibility/material queues and foundations remain unaccepted.
- **Next art baseline:** see `Docs/Design.md` §8.0.3: MO 1.6's `DankPyon_Trench` **壕** and its earth-cut connected atlas are a *verified visual reference*, not a dependency or approved copied asset. Wet and dry should share the same sculpted banks.
- `Bridgeable` permits a Vanilla foundation and the connectivity E2E validates state preservation; screenshots, not a Def assertion, must establish bridge occlusion.

**Visual approval gate before another round of renderer changes:** inspect and agree on a dry/wet side-by-side proposed look and connected variants, based on the verified MO reference. Do not run repeated full rendered E2E trials to compensate for an undecided visual target.

## Art review specification (prior to runtime acceptance)

The author's three hand-drawn horizontal dry/wet and vertical/cross sketches are the current contour references. `Docs/Design.md` §8.0.4 is the formal 16-mask shape specification; do not judge the first art sheet by the earlier hard-rectangle mesh alone.

**First image-review sheet (no game startup):** show a common earth-cut bank form, paired DRY/WET examples of horizontal and vertical straight, elbow, T and cross, plus an endcap. Use the same camera scale, ground texture and geometry between state pairs. MO's ordinary earth ditch is prior-art styling, not a copied atlas. Indicate only the bed contents changing between states.

Check the following before art approval:

- The channel is a depressed trench with *visible shoulder, bank slope and narrower bed*, not a solid blue line or full-cell water.
- Straight and rotated forms join exactly at shared cardinal cell edges; disconnected edges are closed with earth.
- An elbow follows one continuous softened L curve; a T and cross have one connected, modestly widened central basin with no internal earth islands or blind water arms.
- Wet fills only the interior bed; dry exposes its dark soil bottom; the identical bank silhouette persists.
- The inlet and a Vanilla bridge are depicted as design details, not as new hydrology/pathing rules.
- Width, bank lighting, color palette and junction widening receive **visual sign-off** before replacing runtime prototypes; no false PASS is inferred from a generated concept or an automated geometry test.

**Selected adaptation:** see §8.0.5. Original terrain material must be explicitly drawn from Waterworks' saved DefName; grayscale relief mask must be transparent over its actual texture. A colored monochrome replacement tile is not equivalent. Inspect matching Soil, Gravel and Rich Soil soil/stone granularity in addition to the shared contour.

**Current status:** contour specification and adaptive material architecture chosen; art proposal and compositing acceptance OPEN.

**2026-10-08 asset-pipeline checkpoint:** the geometric 80px candidate and three independently generated ImageGen atlases were rejected: artificial regularity, missing/duplicated directions and mismatched water vs. excavation. An independent single-mask generator has now produced 16 connected indices and separate grayscale shade/light, shared bed support, and clipped water textures in a downloadable but **UNAPPROVED** ZIP. Its numeric QA (720 assertions on water containment, reachability, directional edges and opposite-side seams) is evidence of mechanical correctness only. It is **not** proof of hand-dug visual quality, compatibility with real Soil/Gravel/RichSoil textures, or working RimWorld SectionLayer rendering. Until actual accepted source art and in-engine composition are established, do not stage these as Workshop textures or claim any VIS PASS. Do not mark VIS-01…VIS-08 passed until real game frames of the implemented approved art are inspected.

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
| VIS-08 | Original terrain DefName is available for representative diggable cells | Soil, Gravel and Rich Soil canal shoulders retain their respective surrounding texture/colors; wet/dry share silhouette; no universal brown square. Missing original records use a documented visual fallback; reloaded maps retain the mapping | OPEN |

## Execution and evidence

1. Reuse the existing 6/6 graph/pawn/save-load E2E and 1+1 existing-save E2E as logic gates; do not rerun them solely to substitute for visual inspection.
2. Prefer a deterministic Pickle/Quickstarts fixture that prepares the above scene and captures **actual rendered frames** into isolated `TestResults` (when a verified capture API is available). Keep render enabled and require zero `[ERROR]` messages. A terrain-Def assertion or screenshot of an off-screen/unrendered map is not visual evidence.
3. Until an actual capture path is verified, a single in-game scene and screenshot set is sufficient for the subjective parts; do not demand repeated manual campaigns. Record the version/commit, Mod list, map zoom and whether water animation was observed.
4. For each VIS item mark PASS / FAIL with image evidence. Do not label all rendering PASS just because E2E passes.
5. VIS-02 has already failed for full-cell materials and remained illegible in the initial thin-mesh capture. Compare a clear dry/wet reference concept based on MO's excavated earth ditch (§8.0.3), approve its geometry/art first, and only then resume focused implementation. Do not re-run the same visual gate with arbitrary width/color guesses.
6. If VIS-05 fails, inspect RimWorld's foundation/terrain render ordering and shader flags before altering network mechanics or inventing a custom bridge.

## Boundaries

No new water source, canal consumer, bridge family, slope simulation or gameplay feature belongs in this visual gate. `Docs/Design.md` remains authoritative for visual scale and rendering semantics. Passing this gate does not establish full external modpack compatibility.
