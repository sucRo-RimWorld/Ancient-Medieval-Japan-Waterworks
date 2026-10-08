# Waterworks Pickle E2E golden path

Production source static audit and compilation were reported successful by the author (0 warnings, 0 errors). The following is a separate live RimWorld E2E gate.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/run-e2e.ps1 -RimWorldDir "D:\SteamLibrary\steamapps\common\RimWorld"
```

The runner compiles the production DLL, Quickstarts and Pickle test assemblies, stages a developer-only `AncientMedievalJapanWaterworks.E2E` Mod, and creates a separate `TestResults/E2E/SaveData` profile. The normal `ModsConfig.xml` remains unchanged.

It launches the standard renderer with Pickle's fast/no-browser mode, a process watchdog, isolated Player.log and JSON summary. Exactly four named scenarios must pass and the runtime log must have **zero [ERROR] lines**.

Initial runtime scope: loaded Defs, orthogonal source/branch graph, 9-cell standing-water threshold, ocean/marsh exclusion, Vanilla bridge foundation preservation and Fill/terrain restoration.

Actual pawn Construction job execution, save/reload persistence and water imagery remain open; do not claim v1 acceptance based only on this initial gate. This run has not yet been performed.
