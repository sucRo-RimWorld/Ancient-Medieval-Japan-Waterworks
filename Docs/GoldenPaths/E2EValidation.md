# Waterworks Pickle E2E golden path

Production source static audit and compilation were reported successful by the author (0 warnings, 0 errors). The following is a separate live RimWorld E2E gate.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/run-e2e.ps1 -RimWorldDir "D:\SteamLibrary\steamapps\common\RimWorld"
```

The runner compiles the production DLL, Quickstarts and Pickle test assemblies, stages a developer-only `AncientMedievalJapanWaterworks.E2E` Mod, and creates a separate `TestResults/E2E/SaveData` profile. The normal `ModsConfig.xml` remains unchanged.

It launches the standard renderer with Pickle's fast/no-browser mode, a process watchdog, isolated Player.log and JSON summary. Exactly eight named scenarios must pass and the runtime log must have **zero [ERROR] lines**.

Runtime scope: loaded Defs, orthogonal source/branch graph, the one-cell-width rule (no newly completed 2×2 canal block while L/T/cross remain legal), Dig/Fill through Vanilla Mud and Marsh (without treating either as a water source), 9-cell standing-water threshold, ocean/marsh exclusion, Vanilla bridge foundation preservation, Fill/terrain restoration, production WorkGiver/JobDriver execution by a Construction pawn, and an actual disk .rws save/reload round trip that verifies saved original TerrainDefs and loaded-map event handling.

After the one-cell-width revision and the static-audit inventory fix, the author previously reported the seven-scenario `Scripts/run-e2e.ps1` completed successfully: **7/7 passed** (prior revision), 0 failed/skipped, and the runner's isolated runtime `[ERROR]` gate remained at **0**. This is author-reported runtime evidence; the reports/logs were not independently uploaded and inspected in this chat. The added wetland eighth scenario and four-terrain disk save/reload extension require a **fresh** Windows 8/8 Pickle run before merge; previous 7/7 is not their evidence. The separate add-to-existing-save test had already passed its isolated 1/1 + 1/1 gate. The save/load scenario uses Pickle's built-in `When I save and reload` (actual temporary .rws, a new Game and Map); it is not an in-memory mock. It does not itself test adding Waterworks to a save created without the mod, or verify narrow-ditch/water rendering. The dedicated add-to-save script and its two-phase procedure are recorded in `Docs/GoldenPaths/AddToExistingSave.md`.

## RimWorld 1.6 / 1.7 unstable compatibility matrix — 2026-10-13

RimWorld 1.7 was announced on 2026-10-12 and is available on Steam's **unstable** beta branch; 1.6 remains the stable baseline. Ludeon's Waylith migration expects most mods to remain source-compatible, but **this is not evidence of working binary/API/renderer or DBH compatibility**. Run separate, isolated installations or verify the exact game version before each run; do not overwrite the author's stable RimWorld install or saves merely to test unstable.

| Profile | Core Pickle E2E | DBH for Medieval irrigating via Waterworks | Required result |
| --- | --- | --- | --- |
| 1.6 stable, Waterworks only | Existing eight core cases, 8/8 | Not applicable | No errors, source and save round trip |
| 1.6 stable, DBH + DBH for Medieval | Eight core cases plus integration | Source-fed sluice without pump/tower | Actual soil-moisture increase; disconnect/reconnect and save/load |
| 1.7 unstable, Waterworks only | Re-run all eight core cases | Not applicable | Compile/load with 1.7 managed assemblies; no errors |
| 1.7 unstable, DBH + DBH for Medieval | Only if the installed DBH/DBH for Medieval versions actually load on 1.7 | Same integration tests with version-specific assemblies | Distinguish upstream DBH not ready from Waterworks defects |

**Acceptance premises:** Waterworks does not require DBH. Its source must remain a real moving-water or eligible 9+ cell standing-water source; both Waterworks and Medieval canals are cardinally connected; a diagonal-only contact must not irrigate. DBH stores/pumps/towers and ordinary consumer PipeNets must remain unchanged by this adapter. When natural Waterworks supply is removed, only that extra irrigation path shuts off; native DBH supply remains valid. Covered channels, multiple contacts, wet/dry, saved topology and existing saves remain in scope.

**Mandatory evidence per version:** exact RimWorld build/version and branch, DBH and DBH for Medieval versions / active load flags, compiled assembly target, named scenarios with expected assertions and observed results, isolated Player.log error count, save/load findings, and screenshot or renderer check where visuals change. Do not label 1.7 compatible or add a 1.7 supported-version manifest entry before the loaded-game matrix actually passes. Unavailable upstream dependencies are **BLOCKED**, not PASS.

**Current result:** none of these 1.7 runs have been executed; the new Medieval adapter has no verified 1.6 runtime PASS either. The old 1.6 core results are retained only with their original version and scope.

## Mud/Marsh pond-margin excavation validation — 2026-10-09

For PR #10 head `5849ba6c94481a27803cf63cdf22cfff75a168bb`, the author ran `Scripts/run-visual-isolated.ps1 -Core` on Windows from a local checkout of the feature branch and reported `EXIT 0`. The checked-in wrapper propagates the real E2E PowerShell exit status. Its core runner requires static audit and C# compilation, exactly **8/8** named Pickle scenarios with zero failures/skips, including the new `Mud`/`Marsh` excavation/exclusion scenario, and no runtime `[ERROR]` lines. Its save/reload scenario now checks restoring original Mud and Marsh in addition to Soil and Gravel. This is author-reported isolated loaded-game E2E evidence, not a separate screen appearance sign-off. PR #10 merged in commit `476d250740e8d0e9b8a5f494e014f3f14d1d0598`. Existing-save *installation* and normal user Mod-set compatibility remain separate tests. Public Workshop release remains on hold.
