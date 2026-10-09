# Accepted canal relief

The author selected the first 16-mask dry set on 2026-10-09 (not the 60%-width revision).
The exact 512px generated masters and 128px production exports are retained here and
under `Textures/Terrain/AMJW/Canal`. `accepted-export.json` preserves the original
export hashes and its historical pre-acceptance QA status.

The 128px canvas represents one map cell; outer shoulder is approximately 40% of
the cell width. The bed is offset 5px screen-down. N=1, E=2, S=4, W=8.

`build_tiles_reference.py` is the unchanged original authoring script, retained for
provenance. Its original output paths are not installation paths. Run the repository
adapter `Scripts/export-canal-bed.py` to regenerate `Source/CanalBedGeometry.cs` from
its contour functions; Python, NumPy and Pillow are required. This adapter does not
modify the accepted images. The generated pixel runs exactly reconstruct the bed.

No water sprite is shipped. The renderer clips the existing Core
`Terrain/Surfaces/WaterShallowRamp` to the bed mesh and overlays the same dry relief.
Keep this development source directory out of Workshop payloads.
