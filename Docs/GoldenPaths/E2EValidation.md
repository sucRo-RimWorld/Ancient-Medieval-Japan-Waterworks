# Waterworks Pickle E2E golden path

Production source static audit and compilation were reported successful by the author (0 warnings, 0 errors). The following is a separate live RimWorld E2E gate.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/run-e2e.ps1 -RimWorldDir "D:\SteamLibrary\steamapps\common\RimWorld"
```

The runner compiles the production DLL, Quickstarts and Pickle test assemblies, stages a developer-only `AncientMedievalJapanWaterworks.E2E` Mod, and creates a separate `TestResults/E2E/SaveData` profile. The normal `ModsConfig.xml` remains unchanged.

It launches the standard renderer with Pickle's fast/no-browser mode, a process watchdog, isolated Player.log and JSON summary. Exactly seven named scenarios must pass and the runtime log must have **zero [ERROR] lines**.

Runtime scope: loaded Defs, orthogonal source/branch graph, the one-cell-width rule (no newly completed 2×2 canal block while L/T/cross remain legal), 9-cell standing-water threshold, ocean/marsh exclusion, Vanilla bridge foundation preservation, Fill/terrain restoration, production WorkGiver/JobDriver execution by a Construction pawn, and an actual disk .rws save/reload round trip that verifies saved original TerrainDefs and loaded-map event handling.

After the one-cell-width revision and the static-audit inventory fix, the author reported the current `Scripts/run-e2e.ps1` completed successfully: **7/7 passed**, 0 failed/skipped, and the runner's isolated runtime `[ERROR]` gate remained at **0**. This is author-reported runtime evidence; the reports/logs were not independently uploaded and inspected in this chat. The separate add-to-existing-save test had already passed its isolated 1/1 + 1/1 gate. The save/load scenario uses Pickle's built-in `When I save and reload` (actual temporary .rws, a new Game and Map); it is not an in-memory mock. It does not itself test adding Waterworks to a save created without the mod, or verify narrow-ditch/water rendering. The dedicated add-to-save script and its two-phase procedure are recorded in `Docs/GoldenPaths/AddToExistingSave.md`.
