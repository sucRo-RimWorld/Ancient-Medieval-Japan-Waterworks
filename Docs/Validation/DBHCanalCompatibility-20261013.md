# PR #19 — DBH for Medieval audit and Windows acceptance

Date: 2026-10-13 JST. This report records independently executed local tests,
not author-reported historic totals or a Steam publication.
Machine-readable named scenarios, active PackageIds and complete log/summary
SHA256 values: [DBHCanalCompatibility-20261013.json](DBHCanalCompatibility-20261013.json).
Raw logs/reports remain under the relative TestResults paths in that JSON.

## Identity and audit

- Production source: PR #19 `d3418249956afb49a828674f12c7fb077072a5be`,
  with main's test harness `6bf6ac6084759059f31e88de4073014d8a190281` integrated.
  This continuation changes tests/runners/evidence, not the approved irrigation
  algorithm, game balance, production artwork or dependencies.
- Shared production DLL: net472, SHA256
  `a27a37f1552075b2fa01bd4658ba77905c780d91155960550452becfec8e99a0`.
  It was built against the actual stable game's managed assemblies and retained
  unchanged during 1.7 runs; no .NET 10 product DLL was substituted.
- The read-only uploaded `sources/DBHforMedieval.dll` is byte-identical to installed
  Workshop `2926675339/1.6/Assemblies/DBHforMedieval.dll`:
  `5132b2577cc1c6033997285ccf64a7e2e7e7ffea17eda0348882ce6e213bdac9`.
- Installed DBH `836308268/1.6/Assemblies/BadHygiene.dll`:
  `511a4ec97d04c232f09e3f1e7efde20c44b66b52563e03fb4359e3823aaab597`.
  The retained decompiled DBH/Medieval reference sources were inspected; installed
  CompPipe/CompSluiceGate were also freshly decompiled and the real loaded API
  members were checked by RimTest. No separate unavailable ZIP was assumed read.
- `CompPipe.pipeNet` is a property; `PlumbingNet.WaterTowers` and
  `CompWaterStorage.WaterStorage` are fields. `CompSprinkler.PipeComp`,
  `usagePerCell`, `fireDamage`, and Medieval private width/length are fields.
  `ActiveCells()` returns the upstream line-of-sight-filtered rectangle.
- Native `TrySpray()` requires width × length × usagePerCell storage, subtracts
  that amount, calls Cooling, adds 5 moisture per active cell and creates its
  normal ripple. IsWaitingForDoer uses the separate native refill-demand total.
  The adapter preserves native sufficient-storage execution, uses only native
  AddMoisture/Cooling/ripple when storage is insufficient, and never registers
  synthetic towers or changes generic PipeNet consumers.
- Stable logs identify **1.6.4871 rev591**, while installed Version.txt says
  **1.6.4871 rev590**. Native unstable logs identify **1.7.4976 rev1043**.
  The author switched Steam between versions after completed runs; game files
  were not switched by the agent. Normal ModsConfig/Prefs and existing saves
  were not edited by the test runners.
- DBH and Medieval still declare 1.6 in installed metadata. On 1.7 their existing
  1.6 DLLs actually loaded and passed the bounded cases. Pickle/RimTest also use
  their existing 1.6 fallback; Quickstarts selects its 1.7 DLL. Provider hashes
  are recorded in JSON. No upstream 1.7 support announcement is inferred.

## Fixes and build result

The initial stable production build already passed **0 warnings / 0 errors**.
A production C# failure was not reproduced. The native 1.7 test build did fail:
CS0012 required old UnityEngine.CoreModule through PickleDriver's MonoBehaviour
base and the unrelated visual Mesh-returning probe. The test-side exact
Post(Action) reflection bridge preserves Unity main-thread dispatch/assertions;
the 1.7 runner compiles only core/integration owning steps, leaving visual probes
on their separate path. Windows PowerShell's ANSI About.xml parsing was corrected
to explicit UTF-8. A new compatibility-launcher logFile omission was corrected
before accepting any runtime result. The final 1.6 preflight again passed
0 warnings / 0 errors, including the optional adapter contract and asset audits.

The offline 1.6/1.7 harness checks passed runtime/provider selection, rejection of
missing providers, wrong PID, empty log and native ERROR, plus the UTF-8/dispatcher
regression guards. The CCTO/Environment portable runtime helper was inspected and
retained unchanged. No rendering bypass or weakened ERROR gate was introduced.

## Matrix

| Profile / exact assertions | 1.6.4871 rev591 | 1.7.4976 rev1043 |
| --- | --- | --- |
| Solo core Pickle | 8/8 | 8/8 |
| DBHOnly core Pickle | 8/8 | 8/8 |
| Medieval core Pickle, water management ON / needs OFF | 8/8 | 8/8 |
| Medieval irrigation including native pawn job, needs OFF | 5/5 | 5/5 |
| Medieval Lite core; partner water Defs absent | 8/8 | 8/8 |
| Solo RimTest | 4/4 | 4/4 |
| Medieval RimTest | 4/4 | 4/4 |

Every accepted run exited normally, failed/skipped=0 and runtime ERROR=0.
RimTest unknown=0 and loaded production assembly path was verified. 1.7 also
checks the native log PID. Both versions used real rendering on an unswitched
private desktop. Raw log hashes, named outcomes and active Mod lists are in JSON;
no old PASS was relabeled as a result for the adapter.

## Test items and assumed specification

| Core Pickle item | Premise / observed assertions |
| --- | --- |
| Loaded Defs | Wet/dry terrain pathCost=10, Bridgeable, not natural water; Dig/Fill designations and jobs loaded. Lite also asserts ES_IrrigationCanal/ES_SluiceGate absent. |
| Graph | Four cardinal directions; diagonal source rejected; supplied T branch dries after a cut and wets after reconnect. |
| Width | L/T/cross legal; a newly completed 2×2 canal block rejected at designation and execution. |
| Standing source | Eight freshwater cells insufficient; nine mixed-depth cells sufficient; shrinking to eight removes supply; ocean/marsh excluded. |
| Wetland excavation | Exact Mud/Marsh diggable, neither is a source; real river supplies; Fill restores the original wetland terrain. |
| Bridge | Vanilla foundation retains canal topology; Fill prohibited below bridge; source cut/restore works; original Gravel restored after foundation removal. |
| Actual construction | Real capable pawn, production WorkGiver and JobDriver perform Dig/Fill and consume designations. |
| Disk persistence | Real .rws reload rebuilds wet/dry supply, event handling and original Soil/Gravel/Mud/Marsh restoration. |

| Medieval Pickle item | Premise / observed assertions |
| --- | --- |
| Native pawn operation | Actual Growing WorkGiver offers ES_OpenSluiceGate; pawn runs upstream JobDriver; actual CompTick increases all active-cell moisture by at least 5 within 1280 ticks, without tower/pump/well. |
| Natural source and cut | Cardinal Waterworks → native building-canal bend → sluice; native TrySpray adds exactly 5 per active cell. Multiple contacts and a covered Waterworks intake still add once. Disconnect returns false / zero delta and removes native demand; reconnect restores it. Needs-disabled profile explicitly verifies actual Thirst/Bladder/Hygiene false and Lite false. |
| Topology rejection | Removing the intermediate native canal breaks irrigation; only-diagonal contact remains false / zero delta. |
| Native-storage precedence | Actual spawned CompWaterStorage temporarily registered to the real partner net as a bounded native-consumption control: storage=100, usage=3×3×0.05=0.45. Each native spray subtracts 0.45; source loss leaves native irrigation valid. Storage=0.1 with natural supply remains 0.1 after fallback. Registration removed in finally. This is not an end-to-end tower construction/plumbing test. |
| Disk persistence | Actual save/reload creates a new loaded map/sluice; irrigation succeeds, source loss yields zero, reconnection resumes. No Map/Comp reference is used across reload. |

RimTest's four items invoke production IsStanding against loaded Defs, verify
canal/bridge/source flags, verify unknown signature rejection with negative
controls, and inspect actual optional Harmony patches and DBH field/property
shapes (or absence without Medieval). Graph/job/save behavior belongs to Pickle;
RimTest is not presented as a synthetic map simulation.

## Limits and disposition

This closes the requested bounded code/build/1.6 and native 1.7 test scope.
It does not newly certify adding the Mod to every existing save, multi-map
removal, full Medieval Overhaul / arbitrary modpacks, ordinary DBH toilet/tower
construction, screenshot appearance or long-term play. The existing separate
1+1 installation gate and author art acceptance retain their prior scopes.
Production public supportedVersions remains 1.6; version is still 0.1.0-dev.
PR #19 integration, Workshop release and 2GAME posting are separate states.
