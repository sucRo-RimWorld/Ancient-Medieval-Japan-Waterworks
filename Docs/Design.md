# AMJ Waterworks Design

> **Authoritative design source for AMJ Waterworks.**
>
> This repository owns the Waterworks design and implementation. Other AMJ repositories should keep only ownership / compatibility summaries and link here for Waterworks details.

## 1. Purpose

AMJ Waterworks represents **visible, gravity-fed open waterways drawing directly from natural surface water**.

Its defining gameplay is:

`natural fresh water -> intake -> dug open canal -> optional gates / culverts -> external use`

Waterworks is not a general plumbing, hygiene, drinking, sewage, irrigation-crop, rice-cultivation, or water-power overhaul.

The mod exists only while this open-waterway construction loop remains meaningfully distinct from existing pipe-network mods.

## 2. Ownership boundary

Waterworks owns:

- natural surface-water intake;
- dug open canals;
- short culvert / covered crossings;
- manual water gates;
- canal network state and visualization;
- ordinary-water / hot-spring-water source attributes;
- a minimal optional integration surface for other mods to query supplied water;
- hot-spring intake / conveyance infrastructure when a compatible hot-spring source exists.

Waterworks does **not** own:

- rice paddies, rice plants, rice items or rice processing;
- generic crop irrigation bonuses;
- wells, pumps, tanks or closed plumbing that duplicate DBH;
- drinking / thirst;
- toilets, bladder need, sewage, hygiene;
- firefighting systems;
- water wheels, mechanical power or electricity;
- generic water-quality simulation;
- flood control;
- erosion / sediment / canal-maintenance simulation.

Rice Cultivation and Hot Springs remain standalone mods and must not require Waterworks.

## 3. v1 minimum game loop

v1 is complete when the player can:

1. place a valid natural-water intake beside an eligible fresh-water terrain;
2. designate and dig an open canal from it;
3. see the canal become visibly supplied when connected;
4. branch the canal using ordinary T / cross junctions;
5. interrupt or restore a branch with a manual water gate;
6. cross walls, gates, roads and floors using a culvert section;
7. inspect supplied / unsupplied state through a Waterworks overlay;
8. allow another optional mod to query whether a location has supplied ordinary or hot-spring water.

Required v1 elements:

- natural-water intake;
- dug open canal;
- culvert;
- manual water gate.

Stone-lined canals are not a v1 completion requirement.

## 4. Network model

Waterworks v1 uses a **binary supplied / unsupplied connectivity network**, not a fluid simulation.

- Connections are orthogonal only: north / east / south / west.
- Diagonal contact alone does not connect canals.
- Any connected component with at least one valid open intake is supplied.
- Multiple valid intakes may feed the same component.
- Closing a gate cuts connectivity at that cell.
- T and cross junctions distribute supply automatically; no dedicated divider building is required.
- The network does not track per-cell volume, pressure, flow rate, velocity, head, slope, evaporation, leakage, demand or attenuation.
- External consumers query supply state; they do not subtract water in v1.
- Flow direction is not a gameplay requirement. A visual hint may use graph distance from an intake, but no exact multi-source flow solver is required.
- Recalculate supply when topology or source validity changes rather than running fluid calculations every tick.

## 5. Water sources

### 5.1 Ordinary water

The default ordinary-water source set consists of explicit compatible **fresh natural surface-water terrains**, such as rivers, streams, lakes and shallow fresh water.

- Ocean / salt water is not an ordinary-water source in v1.
- Do not infer compatibility from DefName substrings.
- Maintain an explicit source registry / extension point.
- Other mods may register compatible water terrains or source objects through optional integration.
- The exact RimWorld 1.6 TerrainDef whitelist must be audited immediately before implementation.

### 5.2 Hot-spring water

Compatible Hot Springs implementations may register a source as hot-spring water.

The network distinguishes at least:

- ordinary water;
- hot-spring water;
- mixed / ordinary-qualified supply.

If an otherwise isolated hot-spring component becomes connected to an ordinary-water source, it no longer qualifies as pure hot-spring supply for external effects. Players may isolate systems with gates.

Waterworks does not simulate continuous temperature or chemistry.

## 6. Natural-water intake

The baseline intake is a **1x1 shore-side structure**.

- At least one orthogonally adjacent cell must be a registered valid source.
- The intake must also connect orthogonally to the Waterworks network.
- No pump, electricity or pressure is required.
- Intake orientation may affect graphics but is not a hydraulic direction requirement.
- Intakes may be damaged like ordinary buildings.
- Waterworks does not add a generic well or pump.

## 7. Dug open canal

The baseline canal is a visible one-cell-wide dug channel.

### Construction

- Use a **Dig canal** designation rather than placing a normal building.
- Baseline dug canal requires work but no construction material.
- Store the replaced terrain so fill-in can restore it where practical.
- Do not destructively convert every filled canal to Soil.
- Existing roads / floors that must remain intact should use a culvert crossing instead.

### Terrain behavior

- Open canals are walkable shallow channels.
- They impose a meaningful movement penalty compared with ordinary ground.
- Exact movement cost is set during implementation by comparison with Vanilla shallow-water terrain.
- They are not fast paths and do not replace roads.
- Walls, major buildings and ordinary floors do not coexist directly on the open-canal cell.
- Canal terrain itself has no HP; remove it by filling it in.
- Intake and gate buildings remain normal damageable structures.

### Explicit v1 exclusions

Do not simulate:

- erosion;
- leakage;
- silt;
- clogging;
- cleaning;
- routine repair;
- water contamination;
- rainfall-dependent flow;
- snow blockage;
- freezing-dependent supply shutdown.

Waterworks should not invent a canal-freezing simulation while RimWorld's ordinary natural rivers lack a corresponding continuous freeze system.

Roofs do not affect supply in v1.

## 8. Culverts / covered crossings

Culverts use the same logical network as open canals but represent water passing below a constructed surface.

They exist to preserve **visible open waterways as the default**, not to create an unrestricted hidden pipe network.

Rules:

- Open canal and culvert cells connect directly with no separate mandatory transition building.
- The transition may render a culvert mouth / inlet graphic for readability.
- Culverts may pass beneath walls, doors / gates, roads and floors without destroying the surface structure.
- Surface movement and ordinary building behavior remain unchanged by the hidden culvert.
- Culverts appear clearly in the Waterworks overlay.
- Culverts may turn or connect as required under a legitimate crossing.
- Do not allow players to replace the entire open-canal network with arbitrary hidden culverts across open ground.
- Baseline placement should therefore be limited to cells occupied by or reserved for a valid surface crossing / built surface, plus the minimum entrance / exit transition needed to connect back to open canal.
- Do not tunnel freely beneath natural rock / mountain as part of the baseline culvert tool. That would be a separate tunneling feature.
- Exact work and small material cost are implementation-balance values, not yet fixed.

## 9. Water gate

The baseline gate is a 1x1 manual control point on the canal network.

- Open: network connects normally through the cell.
- Closed: connectivity is cut at the gate.
- No gate is required for ordinary branching.
- v1 has no automation, schedules, demand control or logic circuits.
- The gate is a normal damageable building.

## 10. Stone-lined canal

Stone-lined canal remains a later optional feature.

If implemented:

- use the same supply model as dug canal;
- do not grant larger flow capacity or introduce another hydraulic simulation;
- keep benefits modest: construction appearance, settlement aesthetics, possibly reduced movement penalty or other small convenience;
- connect progression to generic stoneworking where practical rather than adding a Waterworks research chain;
- never require stone lining for advanced external consumers to function.

## 11. Progression

Waterworks v1 does not need a multi-tier proprietary research tree.

- Dug canal, intake, basic gate and culvert are early-access water-management tools.
- The main cost of a large network is excavation work and occupied route space.
- Intake / gate / culvert may use small amounts of wood or stone, with exact costs fixed through balance tests.
- MO is not required.
- If MO is present, existing research/material concepts may be integrated conditionally, but Base Waterworks must remain standalone.

## 12. External integration contract

External mods should not traverse Waterworks' internal graph themselves.

Waterworks should expose a minimal stable query surface for questions such as:

- Is this cell / building near a supplied Waterworks connection?
- Is that supplied connection ordinary water, pure hot-spring water, or mixed / non-hot-spring-qualified water?

Rules:

- Consumer-specific ranges belong to the consumer mod.
- Waterworks must not contain constants such as paddy irrigation radius.
- Optional integration must fail safely when Waterworks is absent.
- Adding a new source type should not require hard-coding another mod's DefNames into Waterworks core.

## 13. DBH / DBH for Medieval compatibility

DBH is an **official optional compatibility target**, not a dependency.

The intended ownership split is:

### DBH / DBH for Medieval

- wells;
- pumps;
- tanks;
- PipeNet;
- ordinary plumbing;
- sewage;
- hygiene;
- thirst / drinking where enabled;
- sprinkler-style irrigation.

### Waterworks

- direct intake from natural surface water;
- pump-free gravity-fed visible open canals;
- culverts;
- manual canal gates;
- hot-spring conveyance;
- external supplied-water queries.

### 13.1 Overlap audit

The supplied DBH for Medieval 1.6-era assets were audited on 2026-10-07.

Observed implementation:

- `ES_IrrigationCanal` derives from `DubsDirtyPipeBase` and uses `DubsBadHygiene.CompProperties_Pipe`.
- Its pipe mode is DBH `Sewage`; it is not a separate natural-surface-water canal graph.
- `ES_SluiceGate` uses `DubsBadHygiene.CompProperties_Sprinkler` and a per-cell water-usage model.
- `ES_ManualPump` uses DBH pipe / water-pumping components.
- Primitive-well integration adds a DBH `CompProperties_WaterInlet`.
- The DLL contains DBH-oriented symbols such as `PipeNet`, `HygienePipeMapComp`, `IrrigationGrid` and `FindBestIrrigationSource`.

Therefore DBH for Medieval does not eliminate the specific Waterworks niche of **direct natural intake -> visible gravity-fed open canal**.

### 13.2 DBH configuration compatibility

Current DBH 1.6 exposes separate controls for:

- thirst need;
- bladder need;
- hygiene need;
- Lite Mode.

Its own 1.6 localization states that Lite Mode removes **pipes, water and sewage management**, so Lite Mode is not the profile to use when the player wants DBH's water infrastructure.

Waterworks must support the author's intended **DBH water-infrastructure-only style profile**:

- DBH loaded;
- Thirst disabled;
- Bladder need disabled;
- Hygiene need disabled if desired;
- Lite Mode disabled so DBH water management remains available.

Waterworks code and compatibility patches must not assume any of those pawn needs exist.

Required behavior:

- Waterworks core remains fully functional regardless of DBH need settings.
- If DBH water management / PipeNet is available, an optional adapter may activate.
- If DBH Lite Mode removes the required water-management Defs / systems, the adapter must disable itself cleanly while Waterworks itself continues to function.
- Do not re-enable thirst, bladder or hygiene as a side effect of Waterworks compatibility.
- Release testing must include both ordinary DBH configuration and the water-infrastructure-focused profile with pawn needs disabled.

### 13.3 Compatibility direction

Waterworks must not turn its whole canal graph into DBH `PipeNet`.

If useful after current-API audit, provide a **boundary adapter** rather than duplicate networks. The preferred initial direction is one-way supply from a valid Waterworks ordinary-water canal into a DBH-side inlet / storage interface, while leaving DBH's own quality, tank, pressure and consumer rules under DBH ownership.

Do not route DBH sewage into Waterworks in v1.

Do not treat Waterworks hot-spring-qualified water as automatically equivalent to DBH clean drinking water.

Exact DBH API / water-quality semantics must be re-audited against the installed current 1.6 version before implementation; this document fixes the ownership boundary, not unverified API calls.

## 14. Waterworks standalone value

Waterworks is intentionally infrastructure-first.

It does **not** add generic irrigation, firefighting, hygiene, drinking or water power merely to manufacture standalone economic value.

Its v1 success criterion is:

> The player can visibly draw water from natural surface water through a controllable medieval-style open canal network, and other mods can reliably consume that supplied-state information.

If future simplification removes the visible open-canal construction loop and leaves only an abstract water API, Waterworks should be reconsidered rather than retained as a redundant standalone mod.

## 15. Recommended implementation architecture

This section fixes the **implementation shape**, not final class names.

### 15.1 Open canal representation

Use a Waterworks-owned **TerrainDef for the dug channel** as the persistent surface state.

- The TerrainDef represents the excavated ditch itself, not whether it currently contains water.
- Keep movement cost and basic terrain affordances on that TerrainDef.
- Do not swap the TerrainDef back and forth between separate dry/wet terrain every time supply changes.
- Render supplied water as a Waterworks visual overlay / section layer above the canal terrain.
- An unsupplied canal therefore remains the same ditch terrain but renders dry.
- This keeps topology/state changes from repeatedly rewriting the map terrain grid.

For fill-in restoration, persist the replaced TerrainDef for each excavated canal cell. Restoration must validate that the stored terrain is still legal; if another mod has materially changed the cell context, fail safely rather than forcing an invalid terrain.

### 15.2 Culvert representation

Represent culverts as **persistent per-map underground state**, not as a normal TerrainDef or edifice.

Reason:

- culverts must coexist with roads, floors, walls and gates;
- a normal TerrainDef would replace the surface;
- an ordinary building/edifice cannot safely occupy the same cell as every supported surface structure.

A Waterworks MapComponent should therefore own a saved culvert grid / cell set and expose it to:

- construction/removal jobs;
- topology calculation;
- Waterworks overlay rendering;
- external supply queries.

Culvert state remains invisible in normal map rendering except where an entrance/exit mouth should be drawn.

### 15.3 Intake and gate representation

Natural-water intakes and manual gates should remain normal damageable Things / Buildings with Waterworks comps.

- Intake comp reports whether an adjacent registered natural source is valid.
- Gate comp exposes open / closed state.
- Both notify the map network component when their state changes.
- Gate destruction removes/cuts the gate node according to the resulting canal terrain/structure state; do not leave a phantom connection.

### 15.4 Map network component

Use a per-map Waterworks component as the authoritative runtime network manager.

It should own or index:

- open-canal cells;
- culvert cells;
- intake nodes;
- gate nodes and open/closed state;
- source classification;
- connected-component / supplied-state cache;
- original terrain restoration data;
- overlay data needed for player diagnostics.

Topology should be **invalidated by events** such as:

- canal excavation / fill-in;
- culvert build / removal;
- gate open / close / destruction;
- intake spawn / despawn;
- source registration changes.

After invalidation, rebuild connected components by graph traversal when needed. v1 does not require a continuously simulated graph.

Because other mods may alter natural-water terrains without Waterworks receiving a direct event, intake source validity may also be revalidated at a low-frequency safe checkpoint or before a cached source result is reused. Do not compensate by scanning every canal cell every tick.

### 15.5 Graphics and overlay

Use two visual layers:

1. **normal map view**
   - dug ditch terrain always visible;
   - supplied canals visibly contain water;
   - unsupplied canals visibly read as dry channels;
   - intake and gate buildings render normally;
   - culvert mouths may render where open canal transitions below a crossing.

2. **Waterworks overlay**
   - supplied vs unsupplied network;
   - intake nodes and validity;
   - gate state;
   - hidden culvert path;
   - source class where relevant.

The overlay is diagnostic; normal map view should still communicate wet/dry state without requiring it.

### 15.6 External query API shape

Expose a small stable API from Waterworks rather than exposing internal grids directly.

Conceptual queries:

- supplied connection at / near a cell;
- source class for the supplied component;
- optional nearest supplied connection for a caller-defined radius.

Return neutral/no-supply results when no Waterworks map component exists.

Do not expose mutable internal collections and do not require consumer mods to know Waterworks DefNames or graph representation.

### 15.7 Save/load

Persistent save state must include at least:

- culvert cells;
- original TerrainDefs for excavated/restorable canal cells;
- manual gate state if not already saved by the ThingComp;
- any source registrations that are save-specific rather than Def-driven.

Derived values such as connected-component IDs, supplied flags and overlay caches should be rebuilt after load rather than serialized as authoritative state.

### 15.8 Why not reuse DBH PipeNet internally

Do not use DBH PipeNet as Waterworks' internal network even when DBH is loaded.

That would:

- make DBH a practical implementation dependency;
- inherit volume/pipe semantics Waterworks intentionally does not simulate;
- make Waterworks behavior differ structurally between DBH and non-DBH profiles;
- recreate the same overlap identified in DBH for Medieval.

DBH compatibility belongs at an adapter boundary after both independent systems are valid.

## 16. v1 automated validation targets

When implementation begins, automate at least:

- valid / invalid intake placement;
- orthogonal connectivity and no diagonal-only connectivity;
- dry network with no valid intake;
- wet network with one or multiple intakes;
- T / cross branching;
- gate open / close splitting and recombining components;
- open canal <-> culvert connectivity;
- culvert coexistence with supported surface structures;
- rejection of unrestricted culvert placement across open ground;
- fill-in restoration of recorded prior terrain;
- ordinary / hot-spring / mixed source classification;
- topology recalculation after build, fill, gate toggle and source validity changes;
- optional consumer query behavior with Waterworks present and absent;
- DBH absent profile;
- DBH present compatibility profile;
- DBH present with Thirst / Bladder / Hygiene disabled and Lite Mode off;
- DBH Lite Mode profile: Waterworks works, DBH adapter disables safely;
- runtime ERROR = 0.

Use RimTest Redux for graph / state logic where practical and Pickle for loaded-Def / map / placement / integration behavior.

## 17. Open implementation values

Still intentionally unfixed:

- exact intake-compatible Vanilla TerrainDefs;
- exact excavation work;
- exact intake / gate / culvert material costs;
- exact open-canal movement penalty;
- final TerrainDef / ThingDef / C# representation;
- final overlay presentation;
- current DBH 1.6 adapter API details;
- whether stone-lined canal ships in the first public release after v1 functionality is stable.
