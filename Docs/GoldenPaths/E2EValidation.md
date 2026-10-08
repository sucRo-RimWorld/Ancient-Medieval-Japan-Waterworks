# Waterworks Pickle E2E golden path

Production source static audit and compilation were reported successful by the author (0 warnings, 0 errors). The following is a separate live RimWorld E2E gate.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/run-e2e.ps1 -RimWorldDir "D:\SteamLibrary\steamapps\common\RimWorld"
```

The runner compiles the production DLL, Quickstarts and Pickle test assemblies, stages a developer-only `AncientMedievalJapanWaterworks.E2E` Mod, and creates a separate `TestResults/E2E/SaveData` profile. The normal `ModsConfig.xml` remains unchanged.

It launches the standard renderer with Pickle's fast/no-browser mode, a process watchdog, isolated Player.log and JSON summary. Exactly six named scenarios must pass and the runtime log must have **zero [ERROR] lines**.

Runtime scope: loaded Defs, orthogonal source/branch graph, 9-cell standing-water threshold, ocean/marsh exclusion, Vanilla bridge foundation preservation, Fill/terrain restoration, production WorkGiver/JobDriver execution by a Construction pawn, and an actual disk .rws save/reload round trip that verifies saved original TerrainDefs and loaded-map event handling.

The author reported **5/5** passing. The new **6/6** round-trip gate is unverified until rerun. The save/load scenario uses Pickle's built-in `When I save and reload` (actual temporary .rws, a new Game and Map); it is not an in-memory mock. It does not test adding Waterworks to a save created without the mod, and it does not verify narrow-ditch/water rendering. Neither of those remaining checks is covered by the 6/6 gate.
