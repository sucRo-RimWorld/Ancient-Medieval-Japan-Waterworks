# AMJ Waterworks Agent Instructions

This repository is part of the **Ancient & Medieval Japan (AMJ)** project and owns the standalone **AMJ Waterworks** mod.

Before starting work in this repository:

1. Read this file.
2. Read the authoritative coordination log at `main:Docs/Coordination.md`.
3. Read the relevant formal source of truth, beginning with `Docs/Design.md`.
4. Check OPEN / IN PROGRESS coordination items before changing design or implementation.

## VE-first overlap audit

Before designing a new substantial AMJ feature or proposing a separate Mod, audit the Vanilla Expanded (VE) family first for functional overlap and prior art, using `sucRo-RimWorld/Ancient-Medieval-Japan-Project/Docs/Research/ExistingModAudit.md` as the canonical criteria. VE is a comparison priority, not the AMJ design baseline or an automatic dependency: evaluate historical/cultural fit, dependency footprint, unrelated attached content, retention ratio and reuse value before choosing use-as-is, optional compatibility, patch/retexture, prior-art-only, or AMJ implementation.

## Unowned idea staging

When a new AMJ idea may become a separate Mod but does not yet have an owning repository, **record its durable concept, research and roadmap state in `sucRo-RimWorld/Ancient-Medieval-Japan-Project`**. Do not let this runtime repository become the evolving design home merely because the idea was discovered here. Keep only a concise compatibility or ownership-boundary pointer when relevant. Once a dedicated owner repository exists, migrate confirmed design there.

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
- canal-network state and visualization;
- canal crossings/covers only where they are needed to preserve the open-canal system through settlement construction;
- optional future water-control points such as manual gates when demonstrated by gameplay need;
- a minimal optional query surface for consumer mods when a real integration needs it.

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

Waterworks must **not** grow into a general bridge/foundation pack. Ordinary crossings use the Vanilla bridge. If a heavier crossing/cover is needed so structures can remain above an intact canal, Waterworks may own one canal-specific reinforced cover/foundation rather than requiring a large external bridge mod. Generic bridge families and unrelated foundations remain outside scope.

## DBH compatibility rule

Dubs Bad Hygiene is an official optional compatibility target, not a dependency.

- Do not use DBH PipeNet as Waterworks' internal network.
- Do not duplicate DBH wells, pumps, tanks, plumbing, sewage or sprinkler systems.
- Support DBH profiles with Thirst / Bladder / Hygiene disabled while water management remains enabled.
- If DBH Lite Mode removes the required water-management systems, disable only the adapter; Waterworks core must continue to function.
- Re-audit current DBH 1.6 API / water semantics before implementing an adapter.

## GitHub preflight / CI error hygiene (AMJ common)

Follow the project-wide canonical rule in `sucRo-RimWorld/Ancient-Medieval-Japan-Project/AGENTS.md`.

- Before a remote write that can trigger GitHub Actions, inspect the relevant workflow triggers, path filters, required checks, and repository-specific validation path.
- Run deterministic syntax/structure/XML/packaging/script checks before pushing whenever the current environment can do so. Treat GitHub Actions as a regression gate, not the first parser/debug pass.
- Do not use repeated commits, PR pushes, API writes, or Actions runs as an exploratory debugger, and do not publish obviously broken intermediate states merely to learn from CI.
- If CI fails, stop stacking further remote changes on that workstream. Inspect the failing workflow/job/log, identify the concrete cause, validate the correction, then submit one focused fix instead of speculative variants.
- Where appropriate, use narrow branch/path triggers and `concurrency` / `cancel-in-progress` to avoid duplicate or superseded runs. Do not disable meaningful checks merely to suppress notifications.
- Documentation-only or coordination-only changes should not trigger heavy runtime/build workflows unless those files are part of the validated contract.
- Before weakening or excluding a workflow trigger, verify that release, runtime, packaging, and regression coverage remain protected.

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

- https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/blob/main/Docs/WorkshopPackaging.md
- https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/blob/main/Docs/ModDescriptionGuidelines.md

Do not treat repository preparation as live Steam Workshop publication.

## Art / source preservation

When art work begins, follow the shared AMJ art/source-preservation rules. Accepted high-resolution masters belong under `Art/Sources/` and production `Textures/` files are derivatives. Never overwrite the accepted source merely to create a runtime-sized asset.


## Unowned AMJ idea staging

When work in this repository discovers an AMJ idea that may become a separate Mod but does not yet have an owning repository, **do not develop its evolving design here**. Record the concept, research and roadmap state in `sucRo-RimWorld/Ancient-Medieval-Japan-Project` until the author creates/selects an owner repository. Keep only a concise compatibility or ownership-boundary pointer here when it materially affects this repository.

## World Tech Level recommendation (AMJ common)

**Confirmed:** 2026-10-08 JST.

> AMJとして古代～中世に限定した世界を構成する場合は World Tech Level の Medieval 設定を推奨。

This is a conditional recommendation for assembling an era-limited AMJ world, not a mandatory dependency or a prerequisite for using this individual Mod. Distinguish it from feature-specific compatibility/recommendations when preparing public descriptions. Do not claim that Medieval tech filtering guarantees Japanese historical/cultural suitability or removes every inappropriate event.

Canonical policy: [Project architecture — era-limited world recommendation](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/blob/main/Docs/Architecture.md#era-limited-world-recommendation).

The proposed **Ancient & Medieval Japan - World Rules** remains an uncommitted idea in Project `Docs/Ideas.md`; its ownership, filter scope and relationship/dependency to World Tech Level must be decided separately. Do not add global Incident/Quest/Trader/MapGen filtering to this Mod merely because the recommendation exists.

## Shared rules owner — AMJ Project

Project owns all AMJ-common policy. Before applying a shared rule, read the current [SharedRules index](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/blob/main/Docs/SharedRules.md) and the relevant canonical document there. This repository owns only its Mod-specific specification/procedure; do not develop shared rules in Grains or another runtime Mod.

For AMJ Workshop previews (including text-only image ideas), read Project `Docs/WorkshopCoverStyle.md`, `Docs/GoldenPaths/WorkshopCoverPipeline.md` and `Docs/References/AMJ_WorkshopCover_Manifest.md`, and inspect the actual registered Project reference/base/mask. Present a text composition proposal before generating a new cover. An image-idea request alone does not authorize generation. Never regenerate the common pixels or restore an obsolete cover layout.
