# Waterworks canal tile candidate pipeline

**2026-10-09 update:** the author has selected a full 16-mask dry family. Its actual
runtime integration and validation route is [CanalOverlayRendering.md](CanalOverlayRendering.md).
The EW-only restriction below applies to the older debug candidate generator.

This procedure is the default entry point for **connected Waterworks canal art candidates**. It converts the map-tile rule from a prose-only instruction into a deterministic generation/validation gate.

It does **not** make a generated candidate production art. `Docs/Design.md` §8 and `Docs/GoldenPaths/VisualAcceptance.md` remain the visual authority. Author visual acceptance is still required before anything is promoted to `Art/Sources/` or `Textures/`.

## Current bounded stage

Only the **east-west straight (`EW`) baseline** is enabled. Other masks and `-All` are deliberately rejected until the EW silhouette/relief/water appearance receives explicit visual approval. This prevents an image model or a bulk generator from inventing fifteen additional shapes before the base geometry is accepted.

The current renderer uses deterministic **debug material** for the candidate water and achromatic relief. It is a mechanical pipeline proof, not accepted final earth/water painting. Third-party reference pixels are never copied into the output.

## Command

From the Waterworks repository root:

```powershell
./Scripts/build-canal-tile.ps1 -Mask EW
```

Equivalent Python entry point:

```powershell
python Scripts/canal_tile_pipeline.py --mask EW --out TestResults/CanalTiles/EW
```

Output stays under `TestResults/CanalTiles/EW/` and includes:

- `AMJW_Canal_EW_Shadow.png`
- `AMJW_Canal_EW_Highlight.png`
- `AMJW_Canal_EW_Bed.png`
- `AMJW_Canal_EW_Water.png`
- `AMJW_Canal_Straight_EW_Dry.png`
- `AMJW_Canal_Straight_EW_Wet.png`
- `validation.json`

The script stages output in a temporary directory and exports it only after all mechanical checks pass. A failure leaves no newly accepted candidate at the requested output path.

## Enforced checks

The pipeline currently enforces:

- exact 80×80 RGBA PNG output;
- one shared coded EW contour for Dry/Wet;
- exact E/W border-profile equality for shadow, highlight, bed and water layers;
- water alpha contained by the canonical bed support;
- transparent exterior corners rather than an opaque full-cell replacement tile;
- a bounded open E/W water exit;
- deterministic reproduction by SHA-256;
- rejection of unsupported connection masks;
- rejection of bulk `-All` generation before baseline approval.

Run the regression check with:

```powershell
python Tests/test_canal_tile_pipeline.py
```

## Image-generation boundary

Do **not** ask an image generator to draw a completed connected tile family. Image generation, if used later, supplies only appearance material that can be clipped into the already-fixed contour. Topology, exits, Dry/Wet silhouette identity and final PNG export remain deterministic code responsibilities.

The next extension after EW visual approval is to move the accepted silhouette/master under `Art/Sources/`, then add the remaining cardinal masks from the same connection contract and add pairwise N/S and E/W seam validation. Do not unlock those stages merely because the current mechanical test is green.
