# Ancient & Medieval Japan - Waterworks

RimWorld 1.6 art-free development prototype. **The author confirmed a zero-warning C# build and five passing Pickle E2E scenarios. The expanded six-scenario suite is not yet verified; do not publish.**

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

The expanded gate requires **6/6** exact Pickle scenarios and no isolated runtime `[ERROR]` entries. Reports: `TestResults/E2E/Reports`. The first five scenarios were author-reported passing. Scenario six uses Pickle's built-in game save/reload and checks original-terrain restoration on the reloaded map. Visual rendering and adding Waterworks to a pre-existing save remain independent, unverified gates.
