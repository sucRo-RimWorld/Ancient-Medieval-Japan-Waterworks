# Accepted canal overlay implementation and rendered check

The author selected the original 16 dry masks, superseding the EW-only candidate
stage for this asset family. Keep the original canvas scale, 5px downward bed
offset, and N1/E2/S4/W8 ordering. The narrow 60%-width experiment is not used.

`Art/Sources/Terrain/AMJW/Canal` preserves accepted masters; `Textures/Terrain/AMJW/Canal`
contains exact accepted 128px RGBA derivatives. `Tests/test_canal_assets.py` verifies
all 16 hashes, PNG integrity and 256 compatible edge comparisons. The old EW debug
pipeline is historical tooling, not the selected production texture exporter.

`SectionLayer_AMJW_Canal` explicitly draws saved original terrain, followed by
Core's WaterShallowRamp clipped to generated bed rectangles, followed by the shared
transparent relief. No wet PNG is generated. Queues 2400/2401 place substrate and
water after the canal TerrainDefs' 2388/2389 base. Relief uses Map/Transparent.
Covered cells skip the overlay so Vanilla foundations/bridges remain above canals.
Terrain-change invalidation, canal state, saved metadata and gameplay are preserved.

The water material uses TerrainHard with the existing Core texture; it is not the
full river depth/flow shader and no animated-flow claim is made. Original TerrainDef
appearance is restored, not historical per-cell tint/pollution. Missing records use
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

The same built production source also passed the seven core scenarios in
`TestResults/E2E/Reports/summary.json`, runtime ERROR=0, including the no-2x2
width rule, standing-water threshold, actual construction jobs and disk save/reload.
