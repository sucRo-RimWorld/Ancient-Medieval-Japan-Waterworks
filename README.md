# Ancient & Medieval Japan - Waterworks

RimWorld 1.6 art-free development prototype. **Source is not yet built or runtime-tested; do not publish.**

Includes two canal TerrainDefs referencing existing Vanilla soil/river assets, Construction dig/fill designations and jobs, natural-water source checks, connectivity, original-terrain restoration, and Vanilla bridge/foundation coexistence.

Build locally using a .NET Framework 4.7.2 targeting pack:

```powershell
dotnet build Source/AncientMedievalJapanWaterworks.csproj -p:RimWorldDir="D:/SteamLibrary/steamapps/common/RimWorld" -c Release
```

Output goes to `Assemblies/`. Run `python Tests/static_audit.py` for XML/source contract checks; this does **not** replace a game compile, isolated runtime, save/load or visual test.


## Isolated Pickle/Quickstarts E2E

After a successful production build, run from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/run-e2e.ps1 -RimWorldDir "D:\SteamLibrary\steamapps\common\RimWorld"
```

This builds the developer-only test assemblies, stages the `AncientMedievalJapanWaterworks.E2E` test Mod and uses `TestResults/E2E/SaveData` rather than touching the player's normal ModsConfig. Requires the existing Pickle (Workshop 3791648678) and Quickstarts (3793646067) for testing only.

The first gate requires **5/5** exact Pickle scenarios and no isolated runtime `[ERROR]` entries. Reports: `TestResults/E2E/Reports`. The fifth scenario tests actual pawn dig/fill jobs. Save/load and image quality remain independent gates.
