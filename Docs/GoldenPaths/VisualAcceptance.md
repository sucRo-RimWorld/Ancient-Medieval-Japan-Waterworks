# Waterworks v1 visual acceptance

This is the concrete rendering acceptance procedure for the `Docs/Design.md` §7–8 baseline. It owns acceptance evidence, not prototype history or current work status; those belong in `main:Docs/Coordination.md`. `Docs/Design.md` §8 remains authoritative for contour and rendering architecture.

## Visual target and approval gate

- Use `Docs/Design.md` §8.0.3's verified MO 1.6 `DankPyon_Trench` **壕** only as a visual reference, not a dependency or copied asset.
- Dry and wet variants share the same excavated banks; only the recessed bed contents differ.
- Connectivity/state tests do not establish bridge occlusion or rendered appearance; inspect actual frames.

Before another renderer iteration, agree on the dry/wet connected visual target. Do not repeat full rendered E2E runs merely to choose aesthetics.

## Art review specification (prior to runtime acceptance)

The author's three hand-drawn horizontal dry/wet and vertical/cross sketches are the current contour references. `Docs/Design.md` §8.0.4 is the formal 16-mask shape specification; do not judge the first art sheet by the earlier hard-rectangle mesh alone.

**First art review (no game startup):** use the production-format connected candidates required by the shared AMJ map-tile workflow, and present paired DRY/WET horizontal/vertical straight, elbow, T, cross and endcap examples together for comparison. Use the same camera scale, ground texture and geometry between state pairs. MO's ordinary earth ditch is prior-art styling, not a copied atlas. Indicate only the bed contents changing between states.

Check the following before art approval:

- The channel is a depressed trench with *visible shoulder, bank slope and narrower bed*, not a solid blue line or full-cell water.
- Straight and rotated forms join exactly at shared cardinal cell edges; disconnected edges are closed with earth.
- An elbow follows one continuous softened L curve; a T and cross have one connected, modestly widened central basin with no internal earth islands or blind water arms.
- Wet fills only the interior bed; dry exposes its dark soil bottom; the identical bank silhouette persists.
- The inlet and a Vanilla bridge are depicted as design details, not as new hydrology/pathing rules.
- Width, bank lighting, color palette and junction widening receive **visual sign-off** before replacing runtime prototypes; no false PASS is inferred from a generated concept or an automated geometry test.

**Selected adaptation:** see §8.0.5. Original terrain material must be explicitly drawn from Waterworks' saved DefName; grayscale relief mask must be transparent over its actual texture. A colored monochrome replacement tile is not equivalent. Inspect matching Soil, Gravel and Rich Soil soil/stone granularity in addition to the shared contour.

## Minimum reproducible visual scene

Use an isolated map with Waterworks and its supported Vanilla dependencies only. Do not edit the user's active ModsConfig or saves. Use the normal rendering path (never `-nographics`).

Prepare a straight supplied canal from a natural river, a 90-degree bend, a T-junction and a cross-junction, a disconnected dry segment, and Vanilla bridge foundations over (a) a straight canal cell and (b) a junction-adjacent cell. Include ordinary soil and gravel next to the canal for scale and blending comparison. Keep both wet and dry cells visible in the same view and use an identical zoom level for comparable images.

## Acceptance matrix

| ID | Automated state prerequisite | Visual acceptance | Outcome |
| --- | --- | --- | --- |
| VIS-01 | wet/dry Defs load; supplied terrain switched | Wet and dry are unambiguous in normal map view, without debug overlays | PROVISIONAL (author accepts current images for real-play checking; item-specific PASS not established) |
| VIS-02 | cardinal adjacency accepted, diagonals rejected | A one-cell terrain corridor appears as a **noticeably narrower excavated channel**, with surrounding banks; no full-cell river ribbon | PROVISIONAL (author accepts current images for real-play checking; item-specific PASS not established) |
| VIS-03 | T/cross connectivity accepted | Straight, bend, T and cross shapes show no missing center, seam, broken corners or discontinuous water animation | PROVISIONAL (author accepts current images for real-play checking; item-specific PASS not established) |
| VIS-04 | valid freshwater neighbor supplies canal | River/pond mouth has no misleading dry gap or abrupt water-edge artifact | PROVISIONAL (author accepts current images for real-play checking; item-specific PASS not established) |
| VIS-05 | Vanilla Bridge foundation preserves connectivity; BridgeProps are suppressed only for Canal+Bridge | Same plain board top for E/W and N/S; no dangling under-planks or visible bridge/water artifacts in supplied/dry captures; normal non-canal bridge predicate remains unchanged in isolated Pickle test | PASS (2026-10-09; PR #7, Windows visual 1/1 / ERROR=0 by runner exit 0, author screenshots) |
| VIS-06 | source removed / reconnected | Rebuilt wet↔dry state changes are visible without ghost water, stale mesh or retained wet shader effects | PROVISIONAL (author accepts current images for real-play checking; item-specific PASS not established) |
| VIS-07 | Gravel/Soil restoration E2E passed | Filled terrain visually matches surrounding ground; no leftover rim/water artifact | PROVISIONAL (author accepts current images for real-play checking; item-specific PASS not established) |
| VIS-08 | Original terrain DefName is available for representative diggable cells | Soil, Gravel and Rich Soil canal shoulders retain their respective surrounding texture/colors; wet/dry share silhouette; no universal brown square. Missing original records use a documented visual fallback; reloaded maps retain the mapping | PROVISIONAL (author accepts current images for real-play checking; item-specific PASS not established) |

## Current author art decision — 2026-10-09

The author stated that **the current Waterworks images are acceptable for now** and that all development work other than normal in-game playtesting is provisionally complete. Treat the accepted 16-mask trench banks, native water surface and Vanilla-bridge appearance as the **current playtest baseline**. Do not initiate further image generation or aesthetic replacement merely because earlier draft candidates failed.

Matrix outcomes other than the already evidenced **VIS-05 PASS** are now **PROVISIONAL**, not independent technical PASS claims: this is an overall author acceptance to proceed with normal play, not a row-by-row screenshot/animation audit. No new screenshots, long-play evidence or external-mod compatibility results were supplied with this decision. If practical play reveals a flaw, inspect that exact item before making a targeted fix. Ordinary playtesting is the sole currently active acceptance phase; Steam publication and external integrations remain outside this visual approval.

## Execution and evidence

1. Reuse the existing 8/8 graph/pawn/save-load E2E and 1+1 existing-save E2E as logic gates; do not rerun them solely to substitute for visual inspection.
2. Prefer a deterministic Pickle/Quickstarts fixture that prepares the above scene and captures **actual rendered frames** into isolated `TestResults` (when a verified capture API is available). Keep render enabled and require zero `[ERROR]` messages. A terrain-Def assertion or screenshot of an off-screen/unrendered map is not visual evidence.
3. Until an actual capture path is verified, a single in-game scene and screenshot set is sufficient for the subjective parts; do not demand repeated manual campaigns. Record the version/commit, Mod list, map zoom and whether water animation was observed.
4. For each VIS item mark PASS / FAIL with image evidence. Do not label all rendering PASS just because E2E passes.
5. Historical note: VIS-02 failed for discarded full-cell and early thin-mesh candidates. Those failures are superseded by the current author-accepted 16-mask relief and water renderer; do not re-run speculative width/color iterations without a new concrete playtest finding.
6. If VIS-05 fails, inspect RimWorld's foundation/terrain render ordering and shader flags before altering network mechanics or inventing a custom bridge.

## Boundaries

No new water source, canal consumer, bridge family, slope simulation or gameplay feature belongs in this visual gate. `Docs/Design.md` remains authoritative for visual scale and rendering semantics. Passing this gate does not establish full external modpack compatibility.

### VIS-05 scoped appearance acceptance — 2026-10-09

The author supplied `connected.png` and `disconnected.png` from the PR #7 private-desktop test and judged the board-only result acceptable. The prior additional under-planks at the vertical bridge were no longer visible; the horizontal bridge used the same flat board top. The isolated visual runner exited 0 after its hard gates for Pickle 1/1, runtime ERROR=0 and all four required PNGs. Its E2E step tests the **patched, actual-game** BridgeProps predicate for both canal bridge directions and a non-canal Vanilla bridge control.

This accepts only the canal bridge visual behavior (VIS-05), not the other still-OPEN VIS items, external modpacks, a release package, or Workshop publication. PR #7 merged to main as `0da1a656d76be46ae9a831d672d7ae333ee2ff44`.
