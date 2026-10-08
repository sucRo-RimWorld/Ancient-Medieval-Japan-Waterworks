# Waterworks source-validation golden path

This document records the repeatable **static/build** validation procedure for the RimWorld 1.6 Waterworks prototype. It is **not** a runtime acceptance procedure.

1. Check `AGENTS.md`, `main:Docs/Coordination.md`, `Docs/Design.md` and all touched source/Def paths before changes.
2. Inspect repository Actions/CI triggers before writing to GitHub. The current `add-changenote.yml` workflow is path-filtered to release metadata and does not trigger for ordinary source/design/E2E edits.
3. Run `python Tests/static_audit.py` from the repository root.
4. On Windows with the actual RimWorld 1.6 files, run:

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/validate-source.ps1 -RimWorldDir "D:\SteamLibrary\steamapps\common\RimWorld"
   ```

   This runs the static audit and builds `Source/AncientMedievalJapanWaterworks.csproj` against `Assembly-CSharp.dll`. The script is non-interactive and does not launch the game.

5. Fix source/Def or compiler failures before trying a game runtime profile.

## Required game/runtime acceptance (still pending)

Use an isolated, non-interactive Pickle / RimTest Redux profile with runtime log and Waterworks ERROR=0 checks for: source validity (moving and 9-cell standing freshwater), ocean/marsh rejection, four-way graph splits/rejoins, dig/fill and saved original terrain, bridge/Foundation preservation, and save/load. Rendering tests must keep the normal rendering path in a hidden/off-screen environment; do not use `-nographics` for shader tests.

Then use the shortest necessary human visual check to judge wet/dry clarity, T/cross junctions and a Vanilla bridge covering the canal.

## Limitation and regression history

This editing environment has no RimWorld 1.6 managed DLL and no .NET SDK / Mono compiler; **no successful C# build, game run, or screenshot validation is claimed here**.

RimWorld 1.6 removed the old designator `DraggableDimensions` pattern. The `AMJW_CanalLine` category must contain only the built-in `Line` draw style; `Tests/static_audit.py` checks that source contract.
