# Waterworks — adding the mod to a pre-existing Vanilla save

**Current status:** test source and runner are written; this additional two-phase runtime gate is not yet verified. The earlier isolated Waterworks E2E suite is author-reported **6/6 passing**.

Execute from the Waterworks repository root with RimWorld closed:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/run-add-to-save-e2e.ps1 -RimWorldDir "D:\SteamLibrary\steamapps\common\RimWorld"
```

## What the script checks

**Phase A (no Waterworks loaded):** A dedicated Quickstarts/Pickle-only bootstrap Mod generates a Vanilla RimWorld 1.6 game. The step asserts that Waterworks TerrainDefs are absent, prepares Soil and Gravel plus a nearby moving-freshwater source, and asks Pickle to save and reload as `waterworks-before-install` (retaining `Saves/waterworks-before-install.rws`).

**Phase B (cold restart with Waterworks enabled):** In a **new RimWorld process**, load the saved game with Pickle's `Given the save file ... is loaded` step. The test verifies pre-existing Vanilla terrain has not changed spontaneously; the newly initialized Waterworks component can dig supplied/dry canals; a second genuine save/reload preserves the canal states; and filling restores both original Soil and Gravel TerrainDefs.

The script uses only `TestResults/AddToSave/SaveData` and reports under `TestResults/AddToSave/Reports`. It reads but never writes the normal user `ModsConfig.xml` and normal game saves. The scratch save is generated specifically for testing; this test does not edit a player's real colony.

## Pass gate

- Phase A: exactly **1/1** named Pickle scenario, `Player.log` with zero `[ERROR]`.
- A real `Saves/waterworks-before-install.rws` exists before phase B.
- Phase B: exactly **1/1** named Pickle scenario, `Player.log` with zero `[ERROR]`.
- Production Waterworks is absent from the Phase A ModConfig and present in Phase B.

The previously passing 6/6 core suite is unaffected.

## Still outside this gate

- Loading a complex external Mod stack/save, DLC-specific integrations, or compatibility with other terrain overhauls
- Uninstalling Waterworks after it has modified a map
- Water rendering, narrow-ditch textures/overlays, junction clarity and bridge appearance

**No new images are produced by this work.**


## PowerShell syntax validation

`Scripts/validate-source.ps1` invokes PowerShell's own `System.Management.Automation.Language.Parser.ParseFile` for each script under `Scripts/`. This is a preflight check before building and running either E2E phase. The known `$phase:` interpolation failure has been corrected by delimiting the variable as `${phase}:`.

If the parser finds any script error, the suite stops without touching the isolated test profiles. A corrected parser is not proof of passing runtime tests.


## Diagnosing an ERROR after the summary passes

Do **not** rerun the two-process test solely to read the existing failure. Inspect the saved isolated log, using PowerShell from the Waterworks repository root:

```powershell
Select-String -LiteralPath "TestResults/AddToSave/Reports/AfterInstall/Player.log" -Pattern '\[ERROR\]' -Context 0,12 | Format-List | Out-String -Width 240
```

The runner now automatically prints up to five ERROR lines plus 12 subsequent log lines before throwing, so future failures contain their own cause and adjacent stack trace. The Pickle 1/1 result and the ERROR=0 check are separate; both are required to pass. Do not dismiss a real runtime ERROR merely because a scenario's checks completed.
