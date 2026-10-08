# Ancient & Medieval Japan - Waterworks（中世日本 - 水路）

RimWorld 1.6 art-free development prototype. **The author confirmed a zero-warning C# build and 6/6 passing Pickle E2E scenarios. The separate add-to-existing-save and visual gates remain open; do not publish.**

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

The expanded gate requires **6/6** exact Pickle scenarios and no isolated runtime `[ERROR]` entries. Reports: `TestResults/E2E/Reports`. All six scenarios are author-reported passing, including Pickle's real game save/reload and original-terrain restoration. Visual rendering and adding Waterworks to a pre-existing save remain separate gates.


## Install Waterworks into a previously Vanilla-only save (two-process test)

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/run-add-to-save-e2e.ps1 -RimWorldDir "D:\SteamLibrary\steamapps\common\RimWorld"
```

This builds and runs two **separate**, isolated Pickle sessions. The first creates a Vanilla-only scratch save with the Waterworks production Defs absent. The second enables Waterworks, loads the earlier saved game, excavates wet/dry canals, and confirms original-terrain restoration after another real save/load. Results: `TestResults/AddToSave/Reports/BeforeInstall` and `AfterInstall`.

It never edits the player's normal ModsConfig or game saves. Passing both phases establishes the baseline no-Waterworks-to-Waterworks upgrade path; it is not a general compatibility claim or a visual test.
