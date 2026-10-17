# Covered-canal intake — native 1.6/1.7 acceptance closeout (2026-10-17)

## Scope, exact production identity, and installation boundary

- **Player-visible requirement:** Waterworks open/supplied canal may feed a cardinally adjacent irrigation consumer. A Vanilla Bridge/foundation over a supplied canal preserves underground canal supply/through-flow but **prevents withdrawal from that covered contact**. Uncovering re-enables withdrawal. If the Medieval irrigation consumer has a second exposed cardinal wet contact, it continues to draw from it; when all contacts are covered or supply is cut, it cannot irrigate by the Waterworks path. Natural-water classification, DBH native tower/storage priority, and core Waterworks traversal remain unchanged.
- **Waterworks source in tested product:** `main` at `2e46d4188e2b0c3bed83ca558d65ea2b5b2dca60` (semantic change earlier `f243dee031b02ff2fd065d2296839670387bfe64`).
- **Production assembly:** `Assemblies/AncientMedievalJapanWaterworks.dll`, **shared original net472** production DLL, size 45,568 bytes, SHA-256 `1653a9827bb92ee4bfe9ada314fe550e90afb098f9da786f909c479246e27846`. It was built **after the 1.6 solo core PASS**, then shipped in Waterworks PR #34. The **same** DLL bytes were used for all later 1.7 and DBH-loaded tests; only test assemblies were compiled separately as necessary.
- **Isolation:** licensed 1.6/1.7 Linux RimWorld game copies on Project-managed WSL2 self-hosted GitHub runners; disposable per-run saves, native lock, no modifications to normal Windows player Mods/Steam/save configurations. No automatic PR/push native launches.

## Actual native test evidence

| Native loaded profile / exact game | Project Actions | Result, native error gate | Named assertions/meaning |
| --- | --- | --- | --- |
| **1.6 Solo**, `1.6.4871 rev598` | [#39027975054](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/actions/runs/39027975054) | Original core **8/8 PASS**, 0 runtime ERROR, exit0 | Production Defs, cardinal graph, one-cell width, 9-cell freshwater, Mud/Marsh rules, covered bridge intake vs uninterrupted water flow and uncover recovery, real pawn dig/fill, actual save/reload |
| **1.7 Solo**, `1.7.4977 rev1248` | [#39030075023](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/actions/runs/39030075023) | Focused original core bridge **1/1 PASS**, 0 runtime ERROR, exit0 | Same shared DLL loaded; bridge-covered water remains supplied but direct/adjacent withdrawal blocked; removal recovers and gravel restoration works |
| **1.6 DBH + DBH for Medieval**, `1.6.4871 rev598` | [#39030578855](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/actions/runs/39030578855) | Initial original integration **5/6 PASS**, sixth failed; cannot call this run 6/6 | Five unchanged original partner scenarios PASS, while the covered-contact scenario rejected the test-only needs-setting premise |
| **1.6 DBH + DBH for Medieval**, `1.6.4871 rev598` | [#39031077996](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/actions/runs/39031077996) | Only formerly failing covered-contact **1/1 PASS**, native ERROR=0, exit0 | Original owner strict need-flag assert retained; real native `TrySpray` and `IsWaitingForDoer` behave correctly for sole covered contact, uncover, alternate uncovered contact, all covered, lost and restored source |
| **1.7 DBH + DBH for Medieval**, `1.7.4977 rev1248` | [#39030578855](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/actions/runs/39030578855) | Initial covered-contact **0/1** due same unmet needs-setting premise | Not acceptance; partner assemblies did load far enough to reach owner settings assertion |
| **1.7 DBH + DBH for Medieval**, `1.7.4977 rev1248` | [#39031077996](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/actions/runs/39031077996) | Only focused covered-contact **1/1 PASS**, native ERROR=0, exit0 | Original Medieval production integration method acts on actual generated map, not a mock; entire cover/exposed/alternative source scenario confirmed using unchanged production DLL |

### The covered-contact integration test's required checks

Owner `Tests/E2E/WaterworksMedievalSteps.cs`, scenario
`Natural source feeds native sluice once and disconnects safely`:

1. Check actual DBH Hygiene, Bladder and Thirst runtime needs disabled and DBH Lite Mode false, while normal water-management systems are active.
2. An exposed supplied Waterworks canal offers the actual Medieval sluice work and irrigates its native soil-moisture grid (real `TrySpray`).
3. Bridging the only supplied contact leaves the Waterworks tile supplied, but irrigates **zero** and stops native sluice work.
4. Removing bridge restores irrigation without recreating the canal.
5. A second exposed orthogonally connected Waterworks contact restores irrigation while first stays covered.
6. Covering all usable contacts blocks irrigation; uncovering alternative contact restores it.
7. Disconnecting natural freshwater source disables this Waterworks supply; restoring the source enables it.

1.6 original other five scenarios separately check pawn sluice operations and real soil wetting, diagonal/unconnected Medieval canals, native DBH storage priority under source loss, saved/loaded topology, and two-map isolation with actual map removal.

## Real provider provenance and test fixture diagnosis

- **Dubs Bad Hygiene original:** `Dubwise56/Dubs-Bad-Hygiene` Git commit `33283ec22b7a5b2fb2e1b9d5ac23e9bb6d0ebe1a`. Its About declares `3.2.3250`, supports 1.6 and 1.7. 1.6 BadHygiene.dll SHA-256 `511a4ec97d04c232f09e3f1e7efde20c44b66b52563e03fb4359e3823aaab597`; its 1.7 DLL SHA-256 `3b58e443f6984cce28f1e447e8983c67abdde5cd924878b02c5aba6234cb52bd`.
- **DBH for Medieval original:** Steam Workshop item **2926675339**, anonymous SteamCMD retrieval proven by [Project #39030315171](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/actions/runs/39030315171). `1.6/Assemblies/DBHforMedieval.dll` SHA-256 `5132b2577cc1c6033997285ccf64a7e2e7e7ffea17eda0348882ce6e213bdac9`; original partner About lists versions through **1.6**, not 1.7. The **1.7 PASS is demonstrated binary interoperability only**, not claimed publisher 1.7 support.
- Pinned original DBH partner provider archive was transferred unchanged to both native runners from Project Actions #39030578855; its artifact SHA-256 `e945610d6f07c0c97ea30b2d06056b6a186854ca6ca9c490a90ada322fc7a52d`. Original 1.7 Harmony3/Pickle/RimLogging/Quickstarts are from previously approved SHA-pinned release archives.
- The initial runs' isolated `ModSettings` XML did **not** actually disable DBH needs. Both failures occurred at the old strict *actual DBH settings* assertion, **before** the intake behavior check. The follow-up adjusted only the **compiled test assembly**, setting the actual in-game `DubsBadHygiene.Settings` Bladder/Hygiene/Thirst and LiteMode fields to false before reaching the exact same original asserted branch. Explicit 1.6 recorded before: `bladder=True hygiene=True thirst=False lite=False`; after `bladder=False hygiene=False thirst=False lite=False`. The owner Waterworks C#, published DLL, and original feature test expectations were **unchanged**. This is not evidence that an arbitrary installed DBH settings file is automatically interpreted, nor evidence that DBH users must normally disable those needs.

## Acceptance / exclusions

**Accepted for the bounded covered-intake gameplay change:** actual 1.6 Solo, 1.7 Solo, 1.6 real DBH for Medieval, 1.7 real DBH for Medieval, with recorded native process exit0 and runtime ERROR0 on each successful endpoint. 1.6 full six originally did not pass in a *single* run: it is five unchanged cases followed by a targeted independent correction/run of the one affected case. No additional game restarts are necessary solely to consolidate a PASS count.

**Not asserted:** official DBH for Medieval 1.7 package declaration; general compatibility with arbitrary third-party Mod stacks; visual/renderer author acceptance; adding mod to arbitrary existing colony save; unbounded long-play or balance; release package/Workshop upload or public stability promotion. These are independent existing gates, not required to accept the covered-intake fix.
