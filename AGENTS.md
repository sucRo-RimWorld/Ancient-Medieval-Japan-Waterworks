# AMJ Waterworks Agent Instructions

## Start here

1. Read this file and `main:Docs/Coordination.md`; locate the latest relevant owner/status/evidence, including later corrections. Historical entries are not current approval.
2. Read Project [AGENTS.md](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/blob/main/AGENTS.md) and [Docs/SharedRules.md](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/blob/main/Docs/SharedRules.md): apply its stop conditions, then open only the task-relevant canonical procedures.
3. Read the local specification and affected source/tests below. Shared rules are owned by Project; this file owns only local scope and routing. Missing access or conflicting authority blocks the dependent action, not unrelated safe work.

New features cannot enter implementation before the Project [existing-Mod audit gate](https://github.com/sucRo-RimWorld/Ancient-Medieval-Japan-Project/blob/main/Docs/Research/ExistingModAudit.md#implementation-entry-gate) covers VE and non-VE alternatives and records why independent implementation is needed. Existing approved behavior is not redesigned by this rule audit.

## Local task routes and stops

- Specification: `Docs/Design.md`; preserve the ownership and DBH boundary below.
- Source validation: `Docs/GoldenPaths/SourceValidation.md`; runtime: `Docs/GoldenPaths/E2EValidation.md`; real existing-save addition: `Docs/GoldenPaths/AddToExistingSave.md`.
- Appearance: `Docs/GoldenPaths/VisualFixture.md` and `Docs/GoldenPaths/VisualAcceptance.md`. Successful graph/save tests do not establish rendered canal appearance or bridge occlusion.
- Use RimTest Redux for graph/state logic where practical and Pickle for loaded Defs, placement, integration and save/load; follow Project's non-interactive rendering and runtime ERROR gates.
- Accepted high-resolution masters belong under `Art/Sources/`; `Textures/` contains derivatives. Apply Project's source-preservation pipeline before art work.
- Release metadata: `Tests/validate_add_changenote.py`; use the actual owning release path, not a nonexistent common payload command.

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
