# Covered-canal withdrawal restriction — native 1.6 acceptance, 2026-10-17

## Identity and result

- **Game:** isolated licensed Linux RimWorld 1.6.4871 rev598 (Version.txt).
- **Environment:** Project-managed `amj-rimworld` self-hosted runner, dedicated owned game root and temporary savedata under Xvfb; no normal player save/mod configuration touched.
- **Owner source:** [Waterworks `4521ef1d8e8d50c4a1618d91d920783c9512d8ee`](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Waterworks/commit/4521ef1d8e8d50c4a1618d91d920783c9512d8ee). Gameplay source unchanged since [`f243dee031b02ff2fd065d2296839670387bfe64`](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Waterworks/commit/f243dee031b02ff2fd065d2296839670387bfe64); this later SHA fixes only the native harness's 1.6 versioned framework DLL selection.
- **Actual run:** [Project Actions #39027975054](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/actions/runs/39027975054), success; canonical `acceptance.json` transported from Project results branch `results/amjw-covered-intake16-39027975054`.
- **Results:** 8/8 named core Pickle scenarios passed; 0 failed, 0 skipped; native `[ERROR]` count 0; native process exit 0; source/static audit PASS.
- **After test:** independent final net472 production build completed. `Assemblies/AncientMedievalJapanWaterworks.dll`, SHA-256 `1653a9827bb92ee4bfe9ada314fe550e90afb098f9da786f909c479246e27846`, 45,568 bytes. The builder checked SHA-256 again before recording the source and transport result.
- **Transfer:** native Project-runner artifact encoded as base64 text then decoded using GitHub's binary Blob API on a Waterworks PR branch; no substitute assembly and no local user-build claim.

## Named behavioral tests and premises

| Core scenario | Required premise |
|---|---|
| Waterworks production Defs load correctly | Installed canal terrain, designations and Def contracts load |
| Four-direction canal branches connect and disconnect | Cardinal canal network propagates and updates supply |
| Canal width stays one cell | Dig operations reject newly constructed 2×2 wide canals while allowing junctions |
| Standing freshwater requires nine adjacent cells | Nine-cell freshwater intake threshold and exclusions apply |
| Mud and Marsh allow excavation without becoming freshwater sources | Wetland excavation remains possible without treating wetlands as fresh water |
| Vanilla bridge preserves canal flow and terrain restoration | **Open wet canal allows intake; bridge foundation leaves it wet and flowing but blocks both direct/adjacent withdrawal; removing bridge restores intake; original terrain restores correctly** |
| Construction pawn completes real dig and fill jobs | Real scheduled construction jobs complete |
| Save and reload restores canal supply and original ground | Saved game regenerates supply and original ground after actual reload |

The core bridge scenario is a live loaded-game test using the new production intake API. It does **not** exercise DBH for Medieval's actual `TrySpray` or `IsWaitingForDoer` Harmony paths.

## Explicit exclusions

No test of **DBH for Medieval covered-contact irrigation**, alternative exposed contacts with the DBH adapter, RimWorld **1.7**, art appearance/manual visual inspection, existing-save add/remove beyond the core save-and-reload test, Workshop package or public release. Historical DBH compatibility PASS from 2026-10-13 is for unchanged prior code; do not apply it to the changed covered-intake behavior. The 1.6 solo acceptance supports the new DLL for the bounded core slice only.

The first Project-host attempt [#39027810754](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/actions/runs/39027810754) failed **before** launching the game because it found multiple versioned Pickle DLLs; it is not a gameplay failure. PR #33 corrected the test-only selector. The successful one-shot run above is the only native gameplay attempt in this task.
