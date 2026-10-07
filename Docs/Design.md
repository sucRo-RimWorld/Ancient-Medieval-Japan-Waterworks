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

Candidate sources include:

- rivers;
- streams;
- ponds / lakes;
- shallow fresh natural water.

Ocean / salt water is not a valid v1 source.

A source is treated as effectively continuous for the binary v1 model. Connecting a canal does not lower, consume or dry the river/pond terrain.

The exact RimWorld 1.6 TerrainDef whitelist must be audited immediately before implementation. Do not infer eligibility from DefName substrings.

### 4.2 Wetland terrain treatment

Wet ground is not automatically a Waterworks source.

For v1:

- **Marshy soil / wet growable soil is not a water source.** It represents saturated ground rather than an open water body.
- **Marsh is also not a water source by default.** Vanilla treats it as a shallow-water-affordance wet terrain, but allowing every marsh patch to supply an unlimited binary canal network would bypass the intended need to reach a river, stream, pond or lake.
- Marshy soil may be excavated into a dug canal when it otherwise satisfies the ordinary excavation rules. Filling the canal should restore the recorded marshy-soil terrain when valid.
- Marsh itself is not converted into a dug canal in the initial implementation. It is already a saturated wet terrain, and converting it would blur the distinction between an existing wetland and a deliberately excavated channel.
- Mud and other wet-looking terrains are not promoted to water sources merely because they are wet or bridgeable. Source eligibility remains an explicit whitelist decision.

This keeps the v1 rule legible:

> open natural fresh water supplies canals; merely wet ground does not.

If a later biome/environment integration needs a real spring, seep, wetland outlet or other source object, register that explicit source rather than treating an entire wetland terrain type as infinite water.

### 4.3 Source registration

Keep source recognition extensible, but do not build a large framework before it is needed.

The initial implementation may use an explicit internal whitelist of Vanilla 1.6 source terrains. Add a stable external registration mechanism only when the first optional source-providing integration actually needs it.

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

For the first implementation, restrict excavation to uncomplicated surface cells:

- no edifice occupying the cell;
- no existing constructed floor that Waterworks would have to destroy implicitly;
- no natural water cell itself;
- no impassable natural rock / mountain tunneling.

Ordinary removable vegetation may be cleared through normal prerequisite work if practical; Waterworks should not create a separate vegetation-removal system.

This keeps the initial tool predictable. Crossings and covered channels can be added later if actual play demonstrates the need.

### 6.2 Original terrain

When a canal is excavated, record the natural terrain it replaced.

The purpose is limited to safe fill-in restoration.

Do not treat this as a general terrain-history system.

### 6.3 Fill canal

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
- Exact path cost is an implementation/balance value and should be compared with current Vanilla shallow-water terrain.
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

## 8. Wet / dry presentation

Normal map view must communicate the state without requiring a diagnostic overlay.

### Dry canal

An unsupplied canal should visibly read as an excavated shallow ditch.

### Wet canal

A supplied canal should visibly read as the same ditch containing water.

Preferred implementation shape:

- keep one persistent dug-canal TerrainDef for the excavation;
- render supplied water as a Waterworks visual layer / overlay;
- do not repeatedly swap TerrainDefs merely because supply changed.

The exact renderer is an implementation decision and may change after a prototype.

Selecting/inspecting a canal should expose a simple status such as **Supplied / Dry** (localized appropriately) so the player can diagnose one cell without a dedicated network overlay.

A dedicated network overlay is optional and should only be added if normal visuals and inspection text prove insufficient.

## 9. Runtime architecture

The architecture should remain proportionate to the tiny v1 model.

### 9.1 Per-map state

A per-map Waterworks component may own:

- known canal cells / connectivity cache;
- supplied / unsupplied component results;
- recorded original terrain for restoration.

Derived component IDs and wet/dry caches should be rebuilt after load rather than treated as permanent authoritative save data.

### 9.2 Invalidations

Invalidate / recalculate when relevant state changes, such as:

- canal excavation;
- canal fill-in;
- a valid adjacent natural-water terrain changing.

Do not scan every canal cell every tick.

Because another mod may alter source terrain without notifying Waterworks, a low-frequency source-validity check is acceptable if implementation requires one.

## 10. Integration philosophy

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

## 11. DBH / DBH for Medieval prior-art finding

The supplied DBH for Medieval 1.6-era assets were audited on 2026-10-07.

Observed:

- `ES_IrrigationCanal` derives from `DubsDirtyPipeBase` and uses DBH `CompProperties_Pipe`.
- `ES_SluiceGate` uses DBH sprinkler behavior.
- `ES_ManualPump` uses DBH pipe / pumping components.
- Primitive-well integration adds a DBH water inlet.

Therefore DBH for Medieval does not directly replace Waterworks' narrow purpose of **direct natural-water connection plus visible open canal**.

Waterworks must nevertheless avoid reimplementing DBH's pumps, tanks, pipes or water consumers.

## 12. Existing-mod / VE audit status

The AMJ project-wide prior-art audit currently classifies Waterworks as **independent implementation continued** because the desired natural-intake / visible-open-canal responsibility is not satisfied by the audited broad medieval packages without importing unrelated systems.

Vanilla Factions Expanded - Medieval 2 remains a broad medieval faction/technology/economy/warfare expansion rather than the Waterworks design baseline. VE remains a comparison priority, not a dependency.

Re-audit current 1.6 alternatives before expanding Waterworks beyond this narrow core.

## 13. Prototype acceptance gate

The first vertical prototype is successful when automated/runtime checks demonstrate:

- a canal disconnected from natural fresh water is dry;
- a canal orthogonally connected to a valid river/pond source is wet;
- diagonal-only source contact does not supply the canal;
- marshy soil does not supply the canal;
- marsh does not supply the canal;
- marshy-soil excavation/restoration works in supported cases;
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

## 14. Post-core decision gates

After the minimal core works in play, add features only in response to demonstrated need.

Candidate order:

1. **first real consumer integration** — likely DBH or another already-existing use;
2. **heavy-structure crossing solution** — first try the single Waterworks reinforced canal cover/foundation; consider a true culvert only if that cannot solve the demonstrated need;
3. **manual water control** — gate only if branch control has actual gameplay value;
4. **additional source classes** — e.g. hot spring, only when a consumer exists;
5. **cosmetic/advanced canal types** — e.g. stone lining, only if they create a worthwhile choice.

None of these is automatically part of v1 merely because it is technically feasible.

## 15. Player-facing terminology

Baseline terminology:

- English: **Dig canal** / **Fill canal** / **Dug canal** / **Supplied** / **Dry**
- Japanese: **水路を掘る** / **水路を埋め戻す** / **素掘り水路** / **通水中** / **乾燥**

Exact localization may be refined for natural UI phrasing, but avoid engineering-heavy words such as pressure, flow rate or pipe network for the core canal.

## 16. Open implementation values

Still intentionally unfixed:

- exact Vanilla 1.6 source TerrainDef whitelist;
- exact excavation and fill work;
- exact canal movement penalty;
- exact TerrainDef / rendering implementation;
- safe fallback when original terrain cannot be restored;
- first consumer integration and its API shape;
- exact material/work/support rules for the future reinforced canal cover;
- whether any feature beyond the minimal core belongs in the first public release.
