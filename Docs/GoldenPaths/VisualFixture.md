# Deterministic Waterworks visual scene — implementation contract

The confirmed source of truth is `Docs/Design.md` §§7–8. This file specifies a **separate** visual-fixture scenario to be implemented against the loaded RimWorld 1.6 API. Keep the accepted six core scenarios and their 6/6 gate unchanged; no modification of the player's Mod configuration or saves is permitted.

## Layout

Use an isolated Quickstarts 50×50 map, locating a revealed, building-free patch at least 17×17 cells (the ordinary core fixture uses only 11×11). On the patch, clear removable test vegetation, normalize the base to Soil and use some Gravel for a background texture reference. Preserve original terrain/foundations for cleanup if the map persists.

Coordinates below are relative to the center of the prepared patch. Positive x points east; positive z points north. Add water using the actual loaded TerrainDefs and dig using the production `CanalMapComponent`. The layout must be visibly contiguous, but not count a diagonal touch as a supply.

| Element | Relative cells | Expected |
| --- | --- | --- |
| Freshwater mouth | (−7,0), (−6,0) | valid `WaterMovingShallow` source |
| Supplied trunk | x=−5…5, z=0 | wet |
| North branch | x=0, z=1…5 | wet; T/cross junction at (0,0) |
| South branch | x=0, z=−1…−4 | wet; cross junction at (0,0) |
| Ninety-degree return | x=4, z=1…3 and x=2…4, z=3 | wet; bend |
| Dry comparison | x=−5…−2, z=−5 | dry and not cardinally touching the supplied network |
| Vanilla bridge | (−2,0), (0,2) | `TerrainDefOf.Bridge` foundation, wet canal preserved below |
| Gravel reference | x=2…5, z=−6 | ordinary unmodified Gravel |

Prepare the exact geometry in a **separate Pickle feature**, not in `waterworks-core.feature`. Assert loaded dry/wet TerrainDef identities, supply state, preserved foundations and no ERROR in the isolated Player.log.

## Capture protocol

- Before committing an automated screenshot implementation, verify the actual Unity/RimWorld 1.6 screenshot method and that its output is created after a rendered frame. ScreenShot API existence alone is not a completed render proof.
- Camera must center on the whole scene; lock zoom and keep game UI/debug overlays out of the visual comparison if practical.
- Capture the *same* scene with source connected, source severed and source restored. The canal layout is unchanged except for the source cells.
- Keep output inside a dedicated `TestResults/Visual/` directory; do not publish raw test screenshots or include them in Workshop packages.
- Gate the test on feature PASS, isolated zero ERROR, expected screenshot files actually written, and image dimensions greater than zero; this is still only evidence collection, not automatic PASS for subjective VIS-01…VIS-07.
- A hidden process is permitted only if Unity actually renders frames. Do not use `-nographics`; if hidden WindowStyle yields no render output, use a verified off-screen display or clearly report the capture blocker instead of faking screenshots.
- Preserve a small human-readable manifest: source commit, map seed, map size, screenshot resolution, active Mods list and screenshot filenames.
- Rendering and image acceptance remain OPEN until screenshots are inspected against `VisualAcceptance.md`.

## Implementation checkpoints

1. Implement and compile the fixture's test C# assembly against real RimWorld/Unity managed DLLs.
2. Add an isolated Pickle visual-feature runner with no changes to the confirmed 6/6 runner or the 1+1 existing-save runner.
3. Verify camera/actual render capture timing and check output existence.
4. Inspect frames for channel width, junction seams and bridge occlusion before revising shaders or authoring art.

No new graphics assets or game-feature design changes are authorized by this test plan.
