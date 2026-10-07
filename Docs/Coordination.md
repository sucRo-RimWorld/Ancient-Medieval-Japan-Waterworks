# AMJ Waterworks Coordination

This file is the authoritative coordination surface for work on **AMJ Waterworks**.

At the start of Waterworks work:

1. Read `AGENTS.md`.
2. Read this file from `main`.
3. Read `Docs/Design.md` and any implementation source relevant to the task.
4. Check OPEN / IN PROGRESS items before starting overlapping work.

This file is for handoff, state and blockers only. Confirmed design belongs in `Docs/Design.md` or implementation sources.

## Status vocabulary

- **OPEN** — needs work
- **IN PROGRESS** — being investigated or implemented
- **BLOCKED** — waiting on a prerequisite
- **DONE** — completed and reflected in the formal source of truth
- **ARCHIVED** — historical context only

## Current coordination items

### MIG-WATERWORKS-001 — establish standalone Waterworks repository

**Requested by:** author (2026-10-07 JST)  
**Owner:** Waterworks design / implementation  
**Status:** DONE — standalone repository created and authoritative design migrated from Grains

Result:
- `AGENTS.md` now owns Waterworks repository workflow and permanent boundaries.
- `Docs/Design.md` is the authoritative Waterworks design.
- The prior temporary design in `Ancient-Medieval-Japan-Grains/Docs/WaterworksDesign.md` must be reduced to a migration pointer so there is no competing source of truth.
- Grains must retain only the Waterworks ownership / compatibility summary.

Transferred v1 baseline:
- natural fresh-water intake;
- visible dug open canals;
- binary supplied / unsupplied four-direction network;
- T / cross automatic branching;
- manual gate cuts connectivity;
- short culverts for built crossings;
- ordinary vs hot-spring source classification;
- DBH optional compatibility without using PipeNet internally;
- Rice Cultivation and Hot Springs remain separate optional consumers;
- no flow/pressure/volume simulation, generic irrigation, thirst/hygiene, flood control, erosion or water power in v1.

Historical source commits in Grains:
- ownership split: `f594c340fad0ee067e7ce07778e68856822402fb`
- Waterworks-only cleanup: `10b13764427dd2abdac916eab1deeba9d07bc6df`
- v1 network baseline: `68ce78db90830fe065399f3385475e36ba0507c6`
- scope/source semantics: `da34186c6bee44774fbe0a70ac1432af77376796`
- construction semantics: `4e51249d8b65ea06bd499ab9d1c3320a11b39bdc`
- progression/integration: `f89cbc2a7ccdbf9667d30d635e38392e85d9cb0f`
- DBH overlap audit: `d612d5402f76af466cf86069e55c6c1d72fb342c`
- terrain/v1 exclusions: `f36e37d8145b13d951e758bb453c17b3667d4a3c`
- dedicated temporary design creation: `79b221786d5d9be66d74dfb0e501b977cba5018c`
- implementation architecture: `770f6b8aeef80d2c1918ef0e853f105fa7fceded`
- DBH water-only compatibility profile: `97e91725edbac96bfe63056afe9be3bee518376d`

**Next action:** implement the smallest vertical prototype only: intake -> dug canal -> wet/dry network state and rendering. After that passes automated tests, add manual gate, then culvert. DBH and Hot Springs adapters come after the independent core is proven.

### PROTO-WATERWORKS-001 — minimal independent canal prototype

**Owner:** Waterworks implementation  
**Status:** OPEN

Scope:
1. choose final packageId / DefName prefix before public implementation;
2. add minimal RimWorld 1.6 About/load structure;
3. implement dug-canal TerrainDef and excavation/fill semantics;
4. implement 1x1 natural-water intake;
5. implement per-map event-driven connectivity cache;
6. render supplied vs dry canal state;
7. automate orthogonal connectivity, no-diagonal connectivity, valid/invalid intake and runtime ERROR=0 checks.

Do **not** add gate, culvert, DBH adapter, Hot Springs adapter, stone lining or consumer gameplay until this vertical slice is green.
