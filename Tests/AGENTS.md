# Tests agent instructions — AMJ Waterworks

Read root `AGENTS.md`, `main:Docs/Coordination.md`, Project shared rules and the relevant owner test golden path before test work.

- Use RimTest Redux for canal graph/state logic where practical.
- Use Pickle for actual loaded Defs, placement, integration and save/load.
- Follow Project non-interactive rendering and runtime ERROR gates.
- Source validation: `Docs/GoldenPaths/SourceValidation.md`; runtime: `Docs/GoldenPaths/E2EValidation.md`; existing-save addition: `Docs/GoldenPaths/AddToExistingSave.md`.
- Graph/save PASS does not establish visible canal appearance or bridge occlusion; preserve visual acceptance gates.
