# Waterworks 1.7 test-only golden path

## RimWorld 1.7 unstable test-only entry

Use `powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/Run-17Tests.ps1 -GameRoot "D:\SteamLibrary\steamapps\common\RimWorld"` **only** for a local 1.7 unstable installation with a separately built, inspected Waterworks production DLL already present. The 1.6 `Scripts/run-e2e.ps1` gate is unchanged. This runner does not compile, replace or modify production DLL or public About.xml.

The runner uses actual `Version.txt`, `RimWorld.exe`, current game DLLs and installed Pickle/Quickstarts, compiling only the existing two test assemblies with the Roslyn test helper. It stages one explicitly marked test-only companion Mod, runs the unchanged eight `waterworks-core.feature` scenarios on a hidden desktop with real rendering, checks the game-native log PID and ERROR=0, checks exact scenario names and counts, preserves normal ModsConfig/Prefs and production DLL hashes, and removes only its own marked fixture. Results are retained under `TestResults/RimWorld17-*/` per run.

**8 required scenarios and assumptions:** loaded canal Defs; four-direction connectivity; one-cell width; nine-cell natural freshwater source; Mud/Marsh excavation without false source; Vanilla bridge flow and ground restore; real pawn dig/fill jobs; save/reload restoration. These do not establish the separate add-to-existing-save 1+1 test, seven visual acceptance criteria, DBH integration or Workshop release. A 1.7 build/test failure is a finding, never a PASS. No 1.7 native execution has been performed by this GitHub-only change.
