# AMJ Waterworks Agent Instructions

This repository is part of the **Ancient & Medieval Japan (AMJ)** project and owns the standalone **AMJ Waterworks** mod.

Before starting work in this repository:

1. Read this file.
2. Read the authoritative coordination log at `main:Docs/Coordination.md`.
3. Read the relevant formal source of truth, beginning with `Docs/Design.md`.
4. Check OPEN / IN PROGRESS coordination items before changing design or implementation.

## Source hierarchy

Use repository sources rather than chat history as the primary source of truth:

- Confirmed specifications, design decisions and accepted values -> `Docs/Design.md` or the relevant code/XML/Def/test source.
- Cross-chat / cross-agent handoff, status, blockers and requests -> `main:Docs/Coordination.md`.
- Permanent operating rules -> `AGENTS.md`.

Do not leave durable decisions only in chat history or only in `Docs/Coordination.md`.

## Cross-workstream coordination

Do not use the user as a messenger between AMJ workstreams.

If another AMJ repository owns the requested work, update that repository's authoritative `main:Docs/Coordination.md`.

The authoritative coordination log exists only on `main`; do not create branch-specific copies.

## Consistency rule

When a design or implementation policy changes, audit related design, code, XML/Defs, compatibility, documentation and tests before applying the change.

## Reporting GitHub changes

Only report a file as updated when the GitHub change was actually committed. Always include the real commit SHA.

## Waterworks ownership boundary

Waterworks owns the natural-surface-water / gravity-open-canal layer:

- natural-water intake;
- dug open canals;
- culverts / covered crossings;
- manual water gates;
- canal-network state and visualization;
- ordinary-water / hot-spring-water source classification;
- a minimal optional query surface for consumer mods;
- hot-spring conveyance infrastructure when a compatible source exists.

Waterworks does **not** own:

- rice paddies, rice plants, rice items or rice processing;
- generic crop irrigation bonuses;
- wells, pumps, tanks or closed plumbing that duplicate Dubs Bad Hygiene;
- drinking / thirst;
- toilets, bladder, sewage or hygiene needs;
- firefighting;
- water wheels / mechanical power;
- generic water-quality simulation;
- flood control;
- erosion / sediment / maintenance simulation.

Rice Cultivation and Hot Springs are separate standalone mods. Keep integration optional.

## DBH compatibility rule

Dubs Bad Hygiene is an official optional compatibility target, not a dependency.

- Do not use DBH PipeNet as Waterworks' internal network.
- Do not duplicate DBH wells, pumps, tanks, plumbing, sewage or sprinkler systems.
- Support DBH profiles with Thirst / Bladder / Hygiene disabled while water management remains enabled.
- If DBH Lite Mode removes the required water-management systems, disable only the adapter; Waterworks core must continue to function.
- Re-audit current DBH 1.6 API / water semantics before implementing an adapter.

## Automated testing

Prefer RimTest Redux and Pickle; minimize manual testing.

- Use RimTest Redux for graph/state logic where practical.
- Use Pickle for loaded Defs, map placement, runtime integration and save/load behavior.
- Any automated RimWorld launch must capture an isolated runtime log and fail if this mod emits any ERROR-level entry.
- Human testing should be limited to visuals, readability, UI, controls and play feel that automation cannot judge.

## Non-interactive runtime tests

Automated tests that do not require human visual judgment should run without exposing a normal visible RimWorld window.

If rendering is part of the test, preserve the real rendering path through an off-screen/isolated display rather than bypassing it with `-nographics`.

## Mod naming rule

Do not use ASCII `:` or full-width `：` in the mod name. Use ` - ` if a separator is needed.

## Workshop packaging

Subscriber-irrelevant development files must be excluded from Workshop payloads through the repository-root `.rimignore`.

When packaging is implemented, follow the AMJ shared packaging and description guidelines currently maintained in:

- https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Grains/blob/main/Docs/WorkshopPackaging.md
- https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Grains/blob/main/Docs/ModDescriptionGuidelines.md

Do not treat repository preparation as live Steam Workshop publication.

## Art / source preservation

When art work begins, follow the shared AMJ art/source-preservation rules. Accepted high-resolution masters belong under `Art/Sources/` and production `Textures/` files are derivatives. Never overwrite the accepted source merely to create a runtime-sized asset.
