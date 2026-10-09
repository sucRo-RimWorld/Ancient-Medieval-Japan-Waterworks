# Accepted canal overlay implementation and rendered check

The author selected the original 16 dry masks, superseding the EW-only candidate
stage for this asset family. Keep the original canvas scale, 5px downward bed
offset, and N1/E2/S4/W8 ordering. The narrow 60%-width experiment is not used.

`Art/Sources/Terrain/AMJW/Canal` preserves accepted masters; `Textures/Terrain/AMJW/Canal`
contains exact accepted 128px RGBA derivatives. `Tests/test_canal_assets.py` verifies
all 16 hashes, PNG integrity and 256 compatible edge comparisons. The old EW debug
pipeline is historical tooling, not the selected production texture exporter.

`SectionLayer_AMJW_Canal` explicitly draws saved original terrain, followed by
Core's native shallow-river material clipped to generated bed rectangles, followed
by relief on the exact complement of the wet bed. Dry cells retain the complete
accepted image; wet cells omit its dry-floor shadow. No wet PNG is generated.
Queues 2400/2401 place substrate and
water after the canal TerrainDefs' 2388/2389 base. Relief uses Map/Transparent.
Covered cells skip the overlay so Vanilla foundations/bridges remain above canals.
Terrain-change invalidation, canal state, saved metadata and gameplay are preserved.

The water surface clones `WaterMovingShallow.graphic.MatSingle`, preserving Core's
water shader and WaterShallowRamp. Its matching `waterDepthMaterial` renders the
same clipped bed on `SubcameraDefOf.WaterDepth.LayerId`, just as Core Watergen
does for natural water. No artificial darkening or new wet textures are used.
Width-dependent depth filtering can still make a narrow canal look different
from a broad water source; this is not a guarantee of identical screen RGB.
Original TerrainDef appearance is restored, not historical per-cell tint/pollution. Missing records use
Soil only for drawing, never for fabricated save restoration.

Run `Scripts/validate-source.ps1`, then `Scripts/run-visual-isolated.ps1` on Windows.
The latter uses a private Windows desktop without switching the user's desktop,
keeps rendering enabled, creates a separate save-data profile and records fresh
outputs under `TestResults/Visual/<timestamp>`. `TestResults/latest-visual.txt`
identifies the latest run. Four unedited game captures show supplied, disconnected,
restored and all-16 dry gallery states. The gallery uses real Soil, Gravel and
SoilRich. The runner requires 1/1, zero runtime ERRORs and all four captures.
Run `Scripts/run-visual-isolated.ps1 -Core` for the seven gameplay/save regressions.

2026-10-09 visual evidence: `TestResults/Visual/20261009-102859`, 1/1 PASS,
runtime ERROR=0. Water is visible only while supplied, bridges cover it, and the
gallery shows all 16 center masks. These are local loaded-game checks, not
Workshop publication or automatic author aesthetic acceptance.

The preceding implementation also passed the seven core scenarios in
`TestResults/E2E/Reports/summary.json`, runtime ERROR=0, including the no-2x2
width rule, standing-water threshold, actual construction jobs and disk save/reload.

The subsequent native-water correction adds explicit live assertions for both
water surface and WaterDepth submeshes appearing/disappearing with supply, and
static coverage tests proving bed and wet-bank rectangles are disjoint and
cover the complete cell. See current Coordination for its exact capture run.

## Vanilla Bridge over a Waterworks canal

The original bridge itself supplies the desired flat board surface. No copied game texture, new cover Def or rotated asset is needed. In RimWorld 1.6, `SectionLayer_BridgeProps.ShouldDrawPropsBelow` can add extra hanging plank imagery south of a bridge whose underlying canal is bridgeable. A specific Harmony postfix skips **only** that extra graphical part when the foundation is `TerrainDefOf.Bridge` and the cell's actual top terrain is one of the two Waterworks canals. The native bridge/foundation mechanics and other bridges are unchanged. This cosmetic patch requires the Harmony Mod (compile-only `Lib.Harmony.Ref`, not distributed as a DLL).

Check the N/S and E/W bridges side-by-side in `connected.png` and `disconnected.png`, and ensure no extra lower plank remains on the canal immediately south of the vertical bridge. The isolated Pickle scenario also calls the actual patched Vanilla predicate on a bridge over canal and on a temporary non-canal bridge over water; it must return false and true, respectively. An E2E PASS is not author visual sign-off.
