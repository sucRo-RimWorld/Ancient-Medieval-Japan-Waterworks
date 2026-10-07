# AMJ Waterworks Design

> **Authoritative design source for AMJ Waterworks.**
>
> Waterworks owns only the natural-surface-water / open-canal infrastructure layer. Consumer gameplay such as rice cultivation, hot springs, hygiene, drinking, irrigation bonuses, defensive moats or water power belongs elsewhere.

## 1. Product identity

AMJ Waterworks is a small standalone infrastructure mod for **drawing visible open waterways from natural fresh water**.

The defining interaction is deliberately narrow:

`river / pond -> dig canal -> connected canal becomes wet -> disconnect it and it becomes dry -> fill it back in`

The mod should remain useful as a lightweight shared water-infrastructure layer, not grow into a general water simulation.

Waterworks is distinct from Dubs Bad Hygiene (DBH):

- DBH primarily models wells, pumps, storage, PipeNet and consumers.
- Waterworks models direct natural intake and visible gravity-style open channels.
- Waterworks does not use DBH PipeNet internally and does not require DBH.

## 2. Ownership boundary

Waterworks owns:

- recognition of compatible natural fresh-water sources;
- dug open canals;
- supplied / unsupplied canal-network state;
- wet / dry canal presentation;
- canal excavation and fill-in;
- a minimal optional integration surface when a real consumer needs it.

Waterworks does **not** own:

- rice paddies, rice plants, rice items or rice processing;
- generic crop irrigation bonuses;
- wells, pumps, tanks or closed plumbing;
- thirst, drinking, toilets, bladder, sewage or hygiene;
- firefighting;
- water wheels, mechanical power or electricity;
- generic water-quality simulation;
- flood control;
- erosion, silt, leakage or routine canal maintenance;
- dry moats, water moats, bridges, traversal or raid AI.

Rice Cultivation, Hot Springs, DBH compatibility and any future defensive-earthworks mod are consumers / integrations, not extensions of Waterworks' core responsibility.

## 3. Minimal v1 core

### 3.1 Required features

The **v1 core** requires only:

1. a `Dig canal` designation;
2. a one-cell-wide dug-canal terrain;
3. recognition of valid adjacent natural fresh water;
4. four-direction canal connectivity;
5. binary supplied / unsupplied network state;
6. visibly wet supplied canals and visibly dry unsupplied canals;
7. a `Fill canal` / backfill action that restores the previous natural terrain where valid.

That is the complete mandatory gameplay loop.

### 3.2 No mandatory intake building

A separate intake building is **not required for v1**.

A canal component is supplied when at least one canal cell is orthogonally adjacent to a registered valid natural fresh-water source.

This represents a simple cut / intake at the canal mouth without forcing the player to construct a special object merely to connect a ditch to a river or pond.

A dedicated intake structure, weir, headgate or improved intake may be reconsidered later only if it creates a real control, balance or visual benefit.

### 3.3 v1 invariants

The following are deliberate simplifications, not missing simulation:

- natural fresh-water sources do not deplete because a canal touches them;
- canal length does not reduce supply;
- local map elevation / slope is not simulated;
- water does not have to be assigned a flow direction;
- a connected canal cannot be manually shut off in v1 except by changing topology (for example filling a connecting cell);
- wet and dry canals use the same baseline traversal rules in v1;
- no dedicated research is required: basic canal digging is available from the beginning of normal play.

These invariants may change only when a concrete consumer or gameplay problem demonstrates that the extra simulation creates a useful decision.

### 3.4 What is explicitly not required for v1

The following are deferred until a concrete use case requires them:

- manual water gates;
- reinforced canal cover / heavy foundation;
- true culverts / underground crossings if a reinforced cover still cannot solve the actual layout problem;
- stone-lined canals;
- hot-spring source classification;
- dedicated Waterworks overlay;
- public consumer API beyond what the first real integration needs;
- DBH adapter;
- Rice Cultivation adapter;
- Hot Springs adapter;
- any defensive application.

Do not implement these merely because they were present in earlier drafts.

## 4. Natural-water sources

### 4.1 v1 source class

v1 has only one source class: **ordinary fresh natural water**.

The RimWorld 1.6 source audit establishes these Base-game candidates:

**Always-valid moving freshwater**
- `WaterMovingShallow`
- `WaterMovingChestDeep`

These represent river/creek water and are valid regardless of the size of the local moving-water component.

**Standing freshwater candidates**
- `WaterShallow`
- `WaterDeep`

Standing water is valid only when it belongs to a contiguous orthogonally-connected standing-freshwater body of **at least 9 cells**. Count `WaterShallow` and `WaterDeep` together for this test.

The 9-cell minimum is a gameplay abstraction, not a hydrology simulation. Its purpose is to prevent a one-cell puddle from acting as an unlimited permanent source while still allowing small ponds.

Implementation should use a **bounded flood-fill**: starting from the candidate standing-water cell, stop as soon as 9 eligible connected cells are found. Do not enumerate an entire lake merely to prove that it is large enough. Cache/reuse the result where practical until source terrain invalidates it.

**Explicitly invalid as ordinary freshwater sources**
- `WaterOceanShallow`
- `WaterOceanDeep`
- `Marsh`
- `MarshyTerrain`
- `Mud`

DLC or mod-added water terrains are not automatically valid. Add them deliberately through compatibility once their semantics are known.

A valid source is treated as effectively continuous for the binary v1 model. Connecting a canal does not lower, consume or dry the river/pond terrain.

Source checks must use explicit Def identity / registered source semantics, never substring matching such as "contains Water".

### 4.2 Wetland terrain treatment

Wet ground is not automatically a Waterworks source.

For v1:

- **Marshy soil / wet growable soil is not a water source.** It represents saturated ground rather than an open water body.
- **Marsh is not a water source.** Although Vanilla gives it shallow-water/bridge behavior, it represents wetland terrain rather than the open freshwater source Waterworks requires.
- Marshy soil may be excavated into a dug canal when it otherwise satisfies the ordinary excavation rules. Filling the canal should restore the recorded marshy-soil terrain when valid.
- Marsh itself is not converted into a dug canal in the initial implementation. It is already a saturated wet terrain, and converting it would blur the distinction between an existing wetland and a deliberately excavated channel.
- Mud and other wet-looking terrains are not promoted to water sources merely because they are wet or bridgeable. Source eligibility remains an explicit whitelist decision.

This keeps the v1 rule legible:

> open natural fresh water supplies canals; merely wet ground does not.

If a later biome/environment integration needs a real spring, seep, wetland outlet or other source object, register that explicit source rather than treating an entire wetland terrain type as infinite water.

### 4.3 Source registration

Keep source recognition extensible, but do not build a large framework before it is needed.

The initial implementation uses the explicit Vanilla 1.6 source rules above. Add a stable external registration mechanism only when the first optional source-providing integration actually needs it.

Source-region size checks apply only to standing-water sources unless an integration explicitly defines otherwise. A bridge/foundation over an otherwise valid source cell does not erase the underlying water source.

## 5. Network semantics

Waterworks v1 uses connectivity, not fluid simulation.

- Connectivity is north / east / south / west only.
- Diagonal contact does not connect canals.
- A connected component is **supplied** if any canal cell in that component is orthogonally adjacent to a valid natural-water source.
- Otherwise the component is **unsupplied**.
- T and cross junctions connect automatically.
- Multiple natural sources may supply the same component.
- There is no required flow direction.
- There is no per-cell water quantity, pressure, flow rate, velocity, elevation, head, slope, consumption, evaporation, leakage or attenuation.
- A supplied canal does not become less supplied because it is long.
- Supply ignores apparent uphill/downhill direction on the local map; RimWorld has no continuous local elevation field suitable for historical gravity-flow simulation.
- The v1 abstraction is therefore connectivity to natural water, not a claim that every drawn route would be hydraulically possible in reality.
- Recalculate after canal/source topology changes rather than simulating water every tick.

The network deliberately answers one question:

> Is this canal component connected to usable natural fresh water?

Nothing more is required in v1.

## 6. Canal construction

### 6.1 Dig canal

The player uses a `Dig canal` designation.

Baseline rules:

- available from the start without a Waterworks research project;
- requires work;
- requires no construction material;
- produces a one-cell-wide canal terrain;
- does not behave like placing a wall or pipe building;
- should reuse an existing sensible work category rather than add a new Waterworks-specific work type solely for ditch digging.

For the first implementation, the terrain must satisfy all of the following:

- the underlying natural TerrainDef is `Diggable`;
- it is not water/wetland terrain itself;
- it is not `Ice`;
- it is not an artificial floor/foundation;
- it is not a road terrain that Waterworks would silently destroy;
- no edifice occupies the cell;
- it is not impassable natural rock / mountain tunneling.

This property-based rule intentionally supports compatible natural soils without per-mod patches. For example, AMJ Environment's `AMJ_ThinSoil` already exposes `Diggable` and therefore qualifies automatically unless another exclusion applies.

Expected Vanilla examples include soil, rich soil, stony soil/gravel, sand, soft sand, lichen-covered soil and marshy soil. Mud and Marsh do not qualify as ordinary excavation surfaces for v1.

Ordinary removable vegetation may be cleared through normal prerequisite work if practical; Waterworks should not create a separate vegetation-removal system.

This keeps the initial tool predictable. Crossings and covered channels can be added later if actual play demonstrates the need.

### 6.2 Work model

Use **Construction** work for canal earthwork; do not add a Waterworks-specific work type.

Initial balance values:

- Dig canal: **500 work/ticks per cell**
- Fill canal: **300 work/ticks per cell**
- minimum Construction skill: **none**

These are initial implementation values and may be tuned after automated timing/balance checks, but the design intent is fixed: a long canal should cost meaningful labor while remaining cheaper per cell than constructing a Vanilla wooden bridge.

The dig action has no material cost. Fill-in also has no material refund/cost; it is earthwork, not resource conversion.

### 6.3 Original terrain


When a canal is excavated, record the natural terrain it replaced.

The purpose is limited to safe fill-in restoration.

Do not treat this as a general terrain-history system.

### 6.4 Fill canal

The player may designate a canal cell for fill-in.

- Filling requires work.
- The canal disappears.
- Restore the recorded original terrain when it is still valid.
- If the recorded terrain can no longer be restored safely because the map context changed, fail safely or use a deliberately defined fallback rather than forcing an invalid terrain.
- Filling one cell may split the former canal component; affected components then recalculate supplied state.

## 7. Terrain and movement behavior

The dug canal is a shallow, walkable ditch.

### 7.1 Vanilla bridge reuse as crossing / canal cover

Waterworks does **not** add a custom bridge or canal-lid building in v1.

The dug-canal TerrainDef should expose Vanilla's `Bridgeable` terrain affordance so the standard RimWorld 1.6 bridge can be built over one or more canal cells.

For a one-cell-wide canal, the same Vanilla bridge naturally serves two player-facing roles:

- a crossing placed perpendicular to the canal;
- a wooden cover / plank walkway placed along consecutive canal cells.

Waterworks does not create a separate mechanical distinction between "bridge" and "canal lid". Both are the standard Vanilla bridge foundation over an intact canal.

Rationale:

- bridge/foundation construction is already a Vanilla responsibility;
- adding a Waterworks-specific bridge or lid would duplicate an existing general structure;
- Vanilla 1.6 bridges occupy the foundation layer above the underlying natural terrain, so the canal can remain present and connected below;
- standard bridge construction cost, damage, destruction and rebuild behavior remain Vanilla-owned.

Rendering/earthwork rules:

- supplied-water rendering must not draw visibly over the top surface of an existing bridge/foundation;
- destroying/removing the bridge reveals the canal below without changing canal topology;
- `Fill canal` should not silently erase or invalidate a bridge/foundation above it; require removal of the overlying foundation first in v1;
- a bridge/cover does not interrupt canal connectivity or supply;
- Waterworks does not patch the standard bridge into supporting structures it cannot normally support.

The normal Vanilla bridge supports only the structures Vanilla permits on that foundation. Therefore it can solve ordinary crossings and covered walkways, but it is **not** a universal substitute for a future culvert beneath arbitrary heavy buildings or walls. Reconsider a culvert only if real settlement layouts demonstrate that unmet need.

### 7.2 Reinforced canal cover / heavy foundation

If normal settlement play demonstrates a need to keep a canal running beneath walls or other structures that the Vanilla bridge cannot support, Waterworks should first add **one canal-specific reinforced cover/foundation** rather than a separate underground pipe/grid system or a large bridge pack.

Design intent:

- ordinary crossing and wooden covering remain the Vanilla bridge's job;
- the reinforced cover exists only to preserve an intact Waterworks canal beneath heavier construction;
- it should use the same general foundation/layer approach as a bridge where RimWorld 1.6 permits, so the canal TerrainDef and Waterworks connectivity remain underneath;
- it must not become a general-purpose heavy bridge for unrelated water, marsh or open terrain;
- placement should be restricted to Waterworks canal cells, plus only whatever transition cells are technically unavoidable;
- removing/destroying the reinforced cover should reveal the intact canal below;
- filling the canal should require removing the reinforced cover / supported structure first;
- exact stone material, work cost, support affordances and Def implementation remain implementation-audit values.

Player-facing concept may be a **stone canal cover / stone-lined covered channel** rather than an engineering-heavy "heavy bridge". Final naming should fit pre-Edo Japanese waterworks.

This is intentionally a narrow convenience feature. Waterworks does **not** add multiple bridge tiers, decorative bridge families or a general foundation overhaul.

Existing reinforced-bridge/foundation mods such as VE-family architecture content remain prior art and optional coexistence targets, not dependencies. The presence of similar functionality elsewhere does not justify requiring a large external mod for this single Waterworks-specific need.

A true culvert / hidden underground network should be reconsidered only if this simple foundation-layer solution cannot support a real required layout or consumer use case.

- Pawns may cross it.
- It has a meaningful movement penalty compared with ordinary ground.
- It must not function as a road or movement shortcut.
- Initial `pathCost`: **10** for both wet and dry states.
- This is intentionally milder than Vanilla shallow water and milder than a true defensive ditch; Waterworks canals are infrastructure, not a substitute moat.
- Vanilla bridge/foundation covering the canal removes the underlying canal movement penalty in the normal Vanilla way.
- The canal terrain itself has no HP and cannot be destroyed by weapon attacks.
- Removal is an earthwork action: fill it in.

Wet vs dry state uses the same initial path cost in v1. Do not create two movement-balance models until playtesting shows that doing so creates a useful choice.

Waterworks v1 does not simulate:

- freezing shutdown;
- snow blockage;
- erosion;
- leakage;
- sediment;
- cleaning;
- periodic maintenance;
- contamination;
- rainfall-driven flow changes.

## 8. Wet / dry terrain states

Normal map view must communicate the state without requiring a diagnostic overlay.

Use **two Waterworks TerrainDefs representing one excavated canal state machine**:

- `AMJW_DugCanalDry` — excavated but unsupplied ditch;
- `AMJW_DugCanalWet` — the same ditch while its connected component is supplied.

Both are recognized as canal cells by Waterworks and both preserve the same recorded original terrain for fill-in.

### 8.1 State transitions

- Digging completes as a canal cell, then the network recalculates.
- If its connected component is supplied, all canal cells in that component use the wet TerrainDef.
- If supply is broken, affected cells switch to the dry TerrainDef.
- Reconnection switches them back to wet.
- Terrain switching occurs only on topology/source invalidation or load correction, never as a per-tick fluid simulation.
- Foundation/bridge layers above the canal remain untouched when the underlying canal switches wet/dry.

This is preferred over a custom water-render overlay because it keeps v1 rendering and common terrain behavior data-driven and easier to test.

### 8.2 Shared movement/support behavior

Wet and dry canal TerrainDefs must share:

- `pathCost = 10`;
- `Bridgeable`;
- no normal heavy/medium construction support from the bare canal itself;
- no fertility;
- no HP.

The bridge/foundation layer provides normal crossing/support behavior separately.

### 8.3 Wet-only sensory behavior

The wet TerrainDef should reproduce only the obvious shallow-water consequences that improve readability:

- splash effects when traversed if supported cleanly by TerrainDef fields;
- extinguish fire on the cell if supported cleanly;
- soaking-wet traversal thought if this can be inherited without making the canal a generic Vanilla water source.

Do **not** tag the canal as a generic natural `Water` or `River` source merely to obtain those effects. Other mods must not accidentally interpret an artificial canal as a lake, river or fishing water source.

If a Vanilla water side-effect cannot be obtained without broad semantic tags or invasive patches, omit that side-effect from v1 rather than expanding scope.

### 8.4 Dry state

The dry TerrainDef reads as a shallow excavated ditch.

- it does not extinguish fire;
- it does not create soaking-wet effects;
- it still has the same movement penalty as the wet canal.

### 8.5 Inspection

Selecting/inspecting a canal should expose a simple status such as **Supplied / Dry** (localized appropriately) so the player can diagnose one cell.

A dedicated network overlay remains optional and should only be added if the ordinary terrain visuals and inspection text prove insufficient.

## 9. Runtime architecture

The architecture should remain proportionate to the tiny v1 model.

### 9.1 Per-map state

A per-map Waterworks component may own:

- known canal cells / connectivity cache;
- supplied / unsupplied component results;
- recorded original terrain for restoration.

The current dry/wet TerrainDef is **derived presentation state**, not the authoritative source of supply truth. After load, rebuild connectivity/source results and correct any canal TerrainDef whose saved visual state disagrees with the rebuilt network.

Derived component IDs and wet/dry caches should be rebuilt after load rather than treated as permanent authoritative save data.

### 9.2 Invalidations

Invalidate / recalculate when relevant state changes, such as:

- canal excavation;
- canal fill-in;
- a valid adjacent natural-water terrain changing.

Do not scan every canal cell every tick.

Because another mod may alter source terrain without notifying Waterworks, a low-frequency source-validity check is acceptable if implementation requires one.

## 10. Player interaction / UX

### 10.1 Architect placement

Place the two earthwork commands in **Architect -> Orders** rather than creating a dedicated Waterworks architect tab for v1.

Commands:

- **Dig canal / 水路を掘る**
- **Fill canal / 水路を埋め戻す**

A two-command feature does not justify its own category.

### 10.2 Drag behavior

The dig designator should behave like a one-cell-wide construction line:

- single-cell click is allowed;
- click-drag creates a cardinal straight segment;
- corners are created by placing another segment;
- do not create diagonal-only disconnected chains from a diagonal drag;
- do not provide a rectangle/area tool that silently creates broad artificial ponds in v1.

Fill canal follows the same single-cell / straight-segment interaction over existing canal cells.

### 10.3 Designation lifecycle

- A designation is only a work order; terrain does not change until work finishes.
- Canceling an unfinished designation leaves terrain unchanged.
- Completed canal cells recalculate the affected network immediately.
- Fill designations are rejected while a bridge/foundation or supported structure still occupies the canal cell.
- Re-designating an already matching state should be rejected/no-op rather than stacking duplicate work.

### 10.4 Work prerequisites

Ordinary removable plants that prevent the work should be handled through normal construction-style prerequisite clearing where practical.

Waterworks should not add a new plant-cutting job, hauling stage or material delivery requirement for basic excavation.


### 10.5 Feedback for invalid placement

The designator should explain the first relevant rejection reason rather than silently failing. Minimum user-facing cases:

- terrain cannot be dug;
- existing floor/foundation must be removed first;
- road terrain cannot be destroyed by this command;
- edifice blocks excavation;
- target is already a canal;
- Fill canal requires an existing Waterworks canal;
- bridge/foundation/structure must be removed before filling.

Do not expose internal graph or Def terminology in these messages.

## 11. Integration philosophy

Waterworks core must be complete without any integration, even though its standalone economic value is intentionally small.

A future consumer should ask Waterworks only for the minimum fact it needs, typically:

> Is a supplied canal present at / adjacent to this location?

Do not publish a broad framework API before a real consumer defines the need.

### 10.1 DBH

DBH is an official optional compatibility candidate, not a dependency.

The author may use DBH with Thirst / Bladder / Hygiene disabled while retaining DBH water infrastructure. Waterworks must not assume those needs exist.

If a DBH adapter is later implemented:

- Waterworks' canal graph remains independent;
- do not convert canals into DBH PipeNet;
- use a boundary adapter / inlet;
- DBH keeps ownership of storage, PipeNet, quality/quantity and its consumers;
- DBH Lite Mode may remove the systems needed for the adapter; in that case only the adapter disables itself.

The first DBH integration should be designed only after current DBH 1.6 API semantics are re-audited.

### 10.2 Rice Cultivation

Rice Cultivation remains independently playable without Waterworks.

If integrated later, Rice Cultivation owns all paddy-specific conditions and distances. Waterworks only reports supplied canal presence.

### 10.3 Hot Springs

Hot-spring source classification is **not part of v1 core**.

When Hot Springs integration is actually designed, extend source semantics only as far as that integration needs.

### 10.4 Defensive earthworks

Dry moats, water moats, bridges, swimming/climbing traversal, raid pathfinding and breach behavior belong to a separate future defensive-earthworks / fortification responsibility.

Such a mod may consume Waterworks supply state to fill a water moat. Waterworks owns only water delivery, not defense behavior.

## 12. DBH / DBH for Medieval prior-art finding

The supplied DBH for Medieval 1.6-era assets were audited on 2026-10-07.

Observed:

- `ES_IrrigationCanal` derives from `DubsDirtyPipeBase` and uses DBH `CompProperties_Pipe`.
- `ES_SluiceGate` uses DBH sprinkler behavior.
- `ES_ManualPump` uses DBH pipe / pumping components.
- Primitive-well integration adds a DBH water inlet.

Therefore DBH for Medieval does not directly replace Waterworks' narrow purpose of **direct natural-water connection plus visible open canal**.

Waterworks must nevertheless avoid reimplementing DBH's pumps, tanks, pipes or water consumers.

## 13. Existing-mod / VE audit status

The AMJ project-wide prior-art audit currently classifies Waterworks as **independent implementation continued** because the desired natural-intake / visible-open-canal responsibility is not satisfied by the audited broad medieval packages without importing unrelated systems.

Vanilla Factions Expanded - Medieval 2 remains a broad medieval faction/technology/economy/warfare expansion rather than the Waterworks design baseline. VE remains a comparison priority, not a dependency.

Re-audit current 1.6 alternatives before expanding Waterworks beyond this narrow core.

## 14. Prototype acceptance gate

The first vertical prototype is successful when automated/runtime checks demonstrate:

- a canal disconnected from natural fresh water is dry;
- a canal orthogonally connected to valid moving freshwater is wet;
- a 9+ cell standing-freshwater body supplies a canal;
- an 8-cell or smaller standing-freshwater body does not supply a canal;
- `WaterOceanShallow` / `WaterOceanDeep` do not supply a canal;
- diagonal-only source contact does not supply the canal;
- marshy soil does not supply the canal;
- marsh does not supply the canal;
- Diggable eligible natural terrains accept excavation while Ice/water/road/artificial-floor cases are rejected;
- AMJ Environment `AMJ_ThinSoil` qualifies through `Diggable` without a dedicated compatibility patch;
- marshy-soil excavation/restoration works in supported cases;
- dig/fill jobs use the intended Construction work amounts and no material cost;
- Vanilla bridge placement is valid on dug-canal terrain;
- bridge presence does not interrupt canal connectivity/supply;
- canal fill is rejected while an overlying bridge/foundation remains;
- a connected branch becomes wet through the same component;
- breaking the connection dries the disconnected component;
- reconnecting it restores wet state;
- fill-in restores recorded prior terrain in supported cases;
- save/load preserves excavation/restoration state and rebuilds supplied state;
- runtime ERROR count attributable to Waterworks is zero.

Visual manual checks are limited to:

- wet canal reads as water-filled;
- dry canal reads as a ditch;
- transitions and junctions look acceptable;
- Vanilla bridge can be placed on the canal and visually covers the water surface correctly;
- removing/destroying the bridge reveals the intact canal below;
- movement penalty feels understandable.

Do not block this prototype on DBH, gates, culverts, hot springs, stone lining or public API work.

## 15. Post-core decision gates

After the minimal core works in play, add features only in response to demonstrated need.

Candidate order:

1. **first real consumer integration** — likely DBH or another already-existing use;
2. **heavy-structure crossing solution** — first try the single Waterworks reinforced canal cover/foundation; consider a true culvert only if that cannot solve the demonstrated need;
3. **manual water control** — gate only if branch control has actual gameplay value;
4. **additional source classes** — e.g. hot spring, only when a consumer exists;
5. **cosmetic/advanced canal types** — e.g. stone lining, only if they create a worthwhile choice.

None of these is automatically part of v1 merely because it is technically feasible.

## 16. Player-facing terminology

Baseline terminology:

- English: **Dig canal** / **Fill canal** / **Dug canal** / **Supplied** / **Dry**
- Japanese: **水路を掘る** / **水路を埋め戻す** / **素掘り水路** / **通水中** / **乾燥**

Exact localization may be refined for natural UI phrasing, but avoid engineering-heavy words such as pressure, flow rate or pipe network for the core canal.

## 17. Repository / package identity

Baseline implementation identity:

- display name: **Ancient & Medieval Japan - Waterworks**
- packageId: **`sucro.ancientmedievaljapan.waterworks`**
- C# assembly: **`AncientMedievalJapanWaterworks`**
- root namespace: **`AncientMedievalJapan.Waterworks`**
- DefName prefix: **`AMJW_`**
- RimWorld target: **1.6**
- DLC dependency: **none**
- DBH / MO / AMJ Environment dependency: **none**

Harmony should not be added as a dependency unless the prototype proves a required behavior cannot be implemented cleanly through normal Def/Job/MapComponent APIs.

## 18. Save-compatibility target

Initial support target:

- **Adding Waterworks to an existing RimWorld 1.6 save:** should be supported once runtime-tested. Existing terrain is untouched until the player designates canal work.
- **Removing Waterworks from a save that has ever used Waterworks terrain/state:** not supported by default.

Before public release, test adding to an existing save explicitly. Do not advertise safe removal merely because all visible canals were filled; custom map/save state may still make removal unsafe.

## 19. First implementation slice

This design phase is sufficiently specified for a prototype. The first implementation must stop at:

1. About/load metadata and package identity;
2. dry/wet canal TerrainDefs;
3. Dig/Fill designations and Construction jobs;
4. source validation including moving water and 9-cell standing-water rule;
5. event-driven network recalculation;
6. wet/dry TerrainDef switching;
7. Vanilla bridge coexistence;
8. save/load rebuild;
9. automated/runtime ERROR gate.

Do **not** add DBH integration, gates, reinforced covers, hot-spring semantics, public API, special overlays or consumer gameplay until this slice is green.

## 20. Open implementation values

Still intentionally unfixed:

- whether the 9-cell minimum standing-water threshold needs balance tuning after real maps are sampled;
- exact textures / edge presentation for dry and wet canal TerrainDefs;
- safe fallback when original terrain cannot be restored;
- first consumer integration and its API shape;
- exact material/work/support rules for the future reinforced canal cover;
- whether any feature beyond the minimal core belongs in the first public release.
