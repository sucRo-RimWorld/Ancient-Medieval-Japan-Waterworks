# AMJ Waterworks Coordination

Read `AGENTS.md`, this file from main, then the owning design/test procedure.
PR #19 merged as `5d2c75bd93f00ff202cbfb0f86fd6697191e2ed1`. Superseded prototype/art/test chains are
retained in Git history (pre-compaction main `6bf6ac6084759059f31e88de4073014d8a190281`),
not maintained as competing branch logs.

## Current work

### WATERWORKS-COVERED-INTAKE-20261017 — covered canals cannot feed consumers

**Owner:** Waterworks / DBH for Medieval integration
**Status:** NATIVE 1.6 CORE 8/8 PASS; POST-TEST PRODUCT DLL BUILT; 1.7 AND DBH COVERED-INTAKE TESTS OPEN

Waterworks canals still carry water beneath Vanilla Bridge foundations, but a
foundation-covered cell is not a valid withdrawal point for an adjacent consumer.
The existing cardinal optional DBH for Medieval connection must use an exposed
wet canal contact; covering or uncovering a contact changes intake eligibility
without changing canal topology or natural source recognition. Canonical rule:
`Docs/Design.md` §2.1 and §7.1; C#:
`Source/CanalMapComponent.cs`. Existing core and Medieval Pickle scenarios
include exposed/covered/multiple-contact assertions. No automatic RimWorld
test launch. The committed `Assemblies/` production DLL is **unchanged** until
a real 1.6 production compile is available; 1.6/1.7 loaded-game verification
and packaging remain separate.

Follow-up 2026-10-17: PR #30 added `Scripts/native-covered-intake16.py`
and the manually requested `covered-intake-native16.yml` on `main`
(commit `4181596eb2a8cfb28eed4a3ba83e4baafe82a385`).
The route checks the pinned covered-intake source, builds temporary 1.6
production and Pickle/Quickstarts assemblies, runs the eight existing
1.6 core cases once on the isolated Linux host (rendered through Xvfb),
and restores the host's marked staged Mod. Only on actual 8/8,
zero-[ERROR] acceptance does a subsequent production compile push a
single-DLL topic branch for PR integration. It does not run on push/PR.
Attempted exact PR #29 issue-comment trigger was rejected by the
connected tool's safety check; no issue comment or native workflow run
was created. This is an operational dispatch blocker, **not** a test
failure or runtime PASS. DBH for Medieval integration and native 1.7
remain unverified for this change. Do not merge/update a DLL or claim
loaded-game acceptance until genuine matching artifacts exist.

**Superseding result (2026-10-17):** Project-hosted one-shot run
[#39027975054](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/actions/runs/39027975054)
loaded actual Linux RimWorld 1.6.4871 rev598 and executed Waterworks' existing
core Pickle scenarios **8/8 PASS** (no skips/failures), game exit 0 and native
`[ERROR]` count 0. The bridge scenario directly exercises water flowing under
a cover while withdrawal is blocked, and removal restoring intake.
**After the PASS**, the net472 production DLL was rebuilt and SHA-256
`1653a9827bb92ee4bfe9ada314fe550e90afb098f9da786f909c479246e27846`
was verified before binary transfer. Definitive acceptance premises, exclusions
and source/binary provenance: `Docs/Validation/CoveredIntakeNative16-20261017.md`.
The earlier rejected issue-comment and unassigned Waterworks-repo job are
superseded as dispatch blockers; the Project-hosted branch-create route succeeded.
This covers the **1.6 solo core** only: live DBH for Medieval coverage,
1.7, physical appearance and Workshop release are separate and unverified.


### DBH-CANAL-20261013 — optional Medieval sluice compatibility

**Owner:** Waterworks DBH compatibility
**Status:** DONE FOR IMPLEMENTATION / NAMED ACCEPTANCE CONDITIONS ON WINDOWS 1.6 AND 1.7; PUBLIC RELEASE REMAINS SEPARATE

Compared the uploaded DBHforMedieval DLL to the installed DLL (identical SHA256),
read the decompiled DBH/Medieval APIs, and checked their real Harmony targets,
native job execution, soil-moisture grid and storage consumption. Fixed the
1.7 test-build legacy Unity references and PowerShell UTF-8 metadata parsing.
The shared net472 production DLL is preserved; no .NET 10 production port.

Both versions: Solo/DBHOnly/Medieval core Pickle **8/8** each, Medieval irrigation
**6/6** with Thirst/Bladder/Hygiene OFF and water management ON, Medieval Lite
core **8/8** with partner Defs absent, Solo/Medieval RimTest **4/4** each.
The sixth integration case closes acceptance condition 7: real equal-coordinate
networks on two generated maps do not share supply; the actual game map-removal
path clears the removed consumer and Waterworks event subscription while the
retained map still connects/disconnects and irrigates. All accepted logs have
runtime ERROR=0. Production code/DLL remains unchanged. Exact source/provider identity, named
premises, log hashes and scope limits: `Docs/GoldenPaths/E2EValidation.md` and
`Docs/Validation/DBHCanalCompatibility-20261013.md` (+ JSON). The actual game log
reports 1.6.4871 rev591 (Version.txt rev590) / 1.7.4976 rev1043.

**Next:** no mandatory implementation/test work remains in this bounded DBH
compatibility scope. Installed partner Mods still advertise 1.6, so the 1.7 test
is binary interoperability evidence, not upstream declared support. Arbitrary
modpacks and unrelated DBH consumers remain outside the scoped fixture. Generic toilet/PipeNet supply stays
deferred; the source of truth is `Docs/Design.md` section 2.1.

### WATERWORKS-PLAYTEST-READY-028 — ordinary playtest / publication boundary

**Owner:** Waterworks
**Status:** INITIAL CORE AND CURRENT ART PROVISIONALLY ACCEPTED; REAL-PLAY / PUBLIC RELEASE HOLD

Author disposition from 2026-10-09 remains: `0.1.0-dev`, no beta/stable promotion.
Canonical scope: `Docs/Design.md` sections 1.1 and 8.0.6–8.0.7;
`Docs/GoldenPaths/VisualAcceptance.md`. VIS-05 was explicitly accepted after
PR #7 (`0da1a656d76be46ae9a831d672d7ae333ee2ff44`); other appearance rows remain
provisional. Existing-save installation gate previously passed 1+1 at
`a6411c2745b5cf94316dd412a12eb61f3009a2fa`; PR #10 Mud/Marsh core passed 8/8 and
merged `476d250740e8d0e9b8a5f494e014f3f14d1d0598`. These historical scopes do not
become a fresh full-modpack, rendered-appearance or Workshop PASS in PR #19.

### PRIORITY-FOREST-FOODS-20261008 — deferred expansion

**Owner:** Project execution priorities
**Status:** RETAINED

Project `Docs/ImplementationPriorities.md` / `Docs/Roadmap.md` sequence native
fruit trees and edible forest foods after Ironmaking, before the next general
Waterworks/Rice feature-development phase. Current author-authorized PR #19
compatibility work is a bounded exception, not reopening unrelated expansion.

### DES-REINFORCED-COVER-001 — future canal-specific cover

**Owner:** Waterworks
**Status:** APPROVED DIRECTION; IMPLEMENTATION DEFERRED UNTIL DEMONSTRATED NEED

Use ordinary Vanilla Bridge for current crossings. One canal-specific reinforced
cover may be reconsidered only when a demonstrated heavy-structure layout
needs it; no generic bridge/foundation pack or separate culvert grid is approved.
Formal ownership and design: `AGENTS.md`, `Docs/Design.md`.

## WATERWORKS-NATIVE17-HARNESS-20261013

**State:** TEST-ONLY CORRECTION VERIFIED — latest production base `0bd47856a902680b2dd634ef66231c5f702810b4`, Solo/Core 1.7.4976 rev1043 8/8, complete PID log, native ERROR 0. XML-declaration reader, BOM-free Unicode regression and serial native mutex waiting are the bounded changes. This isolated run uses a 1.7 test build of unchanged upstream source, not the shared net472 binary claim. Existing DBH acceptance remains in its own record. Canonical results/premises: `Docs/GoldenPaths/E2EValidation.md`, `Docs/Testing/Native17-20261013.json`. Art, real existing saves and release remain separate.
