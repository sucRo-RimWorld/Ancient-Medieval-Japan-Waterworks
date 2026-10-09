using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AncientMedievalJapan.Waterworks
{
    /// <summary>
    /// Binary four-direction canal supply. The grid index is reconstructed once
    /// on load, then maintained by earthwork and MapEvents.TerrainChanged.
    /// </summary>
    public sealed class CanalMapComponent : MapComponent
    {
        private Dictionary<int, string> originals = new Dictionary<int, string>();
        private readonly HashSet<IntVec3> canalCells = new HashSet<IntVec3>();
        private readonly HashSet<IntVec3> visited = new HashSet<IntVec3>();
        private readonly Queue<IntVec3> queue = new Queue<IntVec3>();
        private readonly List<IntVec3> component = new List<IntVec3>();
        private readonly List<IntVec3> staleCells = new List<IntVec3>();
        private bool subscribed;
        private bool changingCanalTerrain;
        private bool needsRecalculation;

        public CanalMapComponent(Map map) : base(map) { }

        // Rendering lookup only; never synthesize a restore record for old saves.
        public TerrainDef OriginalTerrainAt(IntVec3 cell)
        {
            string name;
            return originals.TryGetValue(map.cellIndices.CellToIndex(cell), out name)
                ? DefDatabase<TerrainDef>.GetNamedSilentFail(name) : null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref originals, "AMJW_originalTerrainByIndex",
                LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && originals == null)
                originals = new Dictionary<int, string>();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            RebuildCanalIndex();
            Recalculate();
            if (map.events != null && !subscribed)
            {
                map.events.TerrainChanged += OnTerrainChanged;
                subscribed = true;
            }
        }

        public override void MapRemoved()
        {
            if (subscribed && map.events != null)
                map.events.TerrainChanged -= OnTerrainChanged;
            subscribed = false;
            base.MapRemoved();
        }

        public override void MapComponentTick()
        {
            // One batched recalculation on the next tick after a source change.
            // No periodic full-map scan and no per-tick fluid simulation.
            if (needsRecalculation)
                Recalculate();
        }

        private void OnTerrainChanged(IntVec3 cell)
        {
            if (changingCanalTerrain || !cell.InBounds(map)) return;
            bool touchedCanal = canalCells.Contains(cell) || IsCanal(cell);
            // Neighboring terrain affects the mouths and four-direction geometry.
            foreach (IntVec3 direction in GenAdj.CardinalDirections)
            {
                IntVec3 adjacent = cell + direction;
                if (adjacent.InBounds(map) && IsCanal(adjacent))
                {
                    touchedCanal = true;
                    break;
                }
            }
            if (touchedCanal) InvalidateVisualAt(cell);

            // Also handles an externally replaced canal cell without retaining a
            // stale graph vertex. Unknown externally created canals cannot be
            // filled safely because no original-terrain record exists.
            if (IsCanal(cell))
                canalCells.Add(cell);
            else
                canalCells.Remove(cell);

            // Even a standing-water cell away from the canal can change whether
            // a small pond meets the nine-cell threshold. Coalesce all terrain
            // notifications rather than attempt unsafe adjacency-only filtering.
            if (canalCells.Count > 0)
                needsRecalculation = true;
        }

        private void RebuildCanalIndex()
        {
            canalCells.Clear();
            for (int index = 0; index < map.cellIndices.NumGridCells; index++)
            {
                IntVec3 cell = map.cellIndices.IndexToCell(index);
                if (IsCanal(cell))
                    canalCells.Add(cell);
            }
        }

        public bool IsCanal(IntVec3 c)
        {
            if (!c.InBounds(map)) return false;
            TerrainDef t = map.terrainGrid.TopTerrainAt(c);
            return t == AMJW_Defs.AMJW_DugCanalDry ||
                   t == AMJW_Defs.AMJW_DugCanalWet;
        }

        private bool OccupiesCanalWidth(IntVec3 c, bool includePendingDig)
        {
            if (!c.InBounds(map)) return false;
            if (IsCanal(c)) return true;
            return includePendingDig && map.designationManager != null &&
                map.designationManager.DesignationAt(c, AMJW_Defs.AMJW_DigCanal) != null;
        }

        private bool WouldExceedOneCellWidth(IntVec3 c, bool includePendingDig)
        {
            // Any continuous two-cell-wide run necessarily contains a filled 2x2
            // block. Reject only the candidate cell that would complete such a
            // block, preserving ordinary corners, T junctions and crosses.
            for (int originX = -1; originX <= 0; originX++)
            {
                for (int originZ = -1; originZ <= 0; originZ++)
                {
                    bool complete = true;
                    for (int dx = 0; dx <= 1 && complete; dx++)
                    {
                        for (int dz = 0; dz <= 1; dz++)
                        {
                            IntVec3 at = c + new IntVec3(originX + dx, 0, originZ + dz);
                            if (at == c) continue;
                            if (!OccupiesCanalWidth(at, includePendingDig))
                            {
                                complete = false;
                                break;
                            }
                        }
                    }
                    if (complete) return true;
                }
            }
            return false;
        }

        public AcceptanceReport CanDesignateDig(IntVec3 c, bool allowWildPlants = false)
        {
            AcceptanceReport basic = CanDig(c, allowWildPlants);
            if (!basic.Accepted) return basic;
            if (WouldExceedOneCellWidth(c, includePendingDig: true))
                return "AMJW_TooWide".Translate();
            return AcceptanceReport.WasAccepted;
        }

        public AcceptanceReport CanDig(IntVec3 c, bool allowWildPlants = false)
        {
            if (!c.InBounds(map) || c.Fogged(map)) return false;
            if (IsCanal(c)) return "AMJW_AlreadyCanal".Translate();

            TerrainGrid grid = map.terrainGrid;
            TerrainDef t = grid.TopTerrainAt(c);
            if (t.IsRoad) return "AMJW_RoadBlocked".Translate();
            if (grid.FoundationAt(c) != null || grid.UnderTerrainAt(c) != null ||
                grid.TempTerrainAt(c) != null || c.GetFirstBuilding(map) != null)
                return "AMJW_Blocked".Translate();
            foreach (Thing thing in c.GetThingList(map))
            {
                // Wild plants are cleared by an ordinary CutPlant prerequisite
                // job. Never silently remove a cultivated crop.
                Plant plant = thing as Plant;
                if (plant != null && (plant.sown || !allowWildPlants))
                    return "AMJW_Blocked".Translate();
                if (thing.def.category == ThingCategory.Building)
                    return "AMJW_Blocked".Translate();
            }
            // A pond is often surrounded by Mud or Marsh. These two specific
            // Vanilla wetlands can be excavated as a passage to actual freshwater,
            // even when their terrain flags lack Diggable or count as water.
            // Do not turn Mud/Marsh into water sources: supply uses a separate
            // explicit natural-freshwater whitelist in this MapComponent.
            bool diggableWetland = t.defName == "Mud" || t.defName == "Marsh";
            // Diggable exists in XML, not as a TerrainAffordanceDefOf field.
            bool diggableSoil = t.affordances != null &&
                t.affordances.Exists(affordance =>
                    affordance != null && affordance.defName == "Diggable");
            if (t.IsFloor || t.IsIce ||
                (!diggableWetland && (t.IsWater || !diggableSoil)))
                return "AMJW_CannotDig".Translate();
            if (WouldExceedOneCellWidth(c, includePendingDig: false))
                return "AMJW_TooWide".Translate();

            return AcceptanceReport.WasAccepted;
        }

        public AcceptanceReport CanFill(IntVec3 c)
        {
            if (!c.InBounds(map) || c.Fogged(map)) return false;
            if (!IsCanal(c)) return "AMJW_NotCanal".Translate();
            if (map.terrainGrid.FoundationAt(c) != null ||
                map.terrainGrid.UnderTerrainAt(c) != null ||
                c.GetFirstBuilding(map) != null)
                return "AMJW_Blocked".Translate();

            string oldDefName;
            if (!originals.TryGetValue(map.cellIndices.CellToIndex(c), out oldDefName) ||
                DefDatabase<TerrainDef>.GetNamedSilentFail(oldDefName) == null)
                return "AMJW_CannotRestore".Translate();
            return AcceptanceReport.WasAccepted;
        }

        public bool Dig(IntVec3 c)
        {
            if (!CanDig(c).Accepted) return false;
            originals[map.cellIndices.CellToIndex(c)] =
                map.terrainGrid.TopTerrainAt(c).defName;
            changingCanalTerrain = true;
            try
            {
                map.terrainGrid.SetTerrain(c, AMJW_Defs.AMJW_DugCanalDry);
            }
            finally
            {
                changingCanalTerrain = false;
            }
            canalCells.Add(c);
            Recalculate();
            InvalidateVisualAt(c);
            return true;
        }

        public bool Fill(IntVec3 c)
        {
            if (!CanFill(c).Accepted) return false;
            int index = map.cellIndices.CellToIndex(c);
            TerrainDef original = DefDatabase<TerrainDef>.GetNamedSilentFail(originals[index]);
            if (original == null) return false;

            changingCanalTerrain = true;
            try
            {
                map.terrainGrid.SetTerrain(c, original);
            }
            finally
            {
                changingCanalTerrain = false;
            }
            originals.Remove(index);
            canalCells.Remove(c);
            Recalculate();
            InvalidateVisualAt(c);
            return true;
        }

        private void InvalidateVisualAt(IntVec3 cell)
        {
            // TerrainGrid already dirties its own cell. Adjacent section meshes
            // also need a redraw when an arm starts/ends at a section boundary.
            if (map.mapDrawer != null)
                map.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Terrain, true, true);
        }

        private static bool IsStanding(TerrainDef t)
        {
            return t.defName == "WaterShallow" || t.defName == "WaterDeep";
        }

        public bool HasNaturalSourceAt(IntVec3 c) { return SourceAt(c); }

        private bool SourceAt(IntVec3 c)
        {
            if (!c.InBounds(map)) return false;
            TerrainDef t = map.terrainGrid.TopTerrainAt(c);
            if (t.defName == "WaterMovingShallow" ||
                t.defName == "WaterMovingChestDeep")
                return true;
            return IsStanding(t) && StandingBodyHasNineCells(c);
        }

        private bool StandingBodyHasNineCells(IntVec3 start)
        {
            // Bounded flood-fill: stop at 9, even for a large lake.
            HashSet<IntVec3> seen = new HashSet<IntVec3> { start };
            Queue<IntVec3> pending = new Queue<IntVec3>();
            pending.Enqueue(start);
            while (pending.Count > 0)
            {
                IntVec3 at = pending.Dequeue();
                if (seen.Count >= 9) return true;
                foreach (IntVec3 d in GenAdj.CardinalDirections)
                {
                    IntVec3 next = at + d;
                    if (!next.InBounds(map) || seen.Contains(next) ||
                        !IsStanding(map.terrainGrid.TopTerrainAt(next))) continue;
                    seen.Add(next);
                    pending.Enqueue(next);
                }
            }
            return seen.Count >= 9;
        }

        public void Recalculate()
        {
            needsRecalculation = false;
            visited.Clear();
            staleCells.Clear();
            changingCanalTerrain = true;
            try
            {
                foreach (IntVec3 start in canalCells)
                {
                    if (!IsCanal(start))
                    {
                        staleCells.Add(start);
                        continue;
                    }
                    if (!visited.Add(start)) continue;

                    queue.Clear();
                    component.Clear();
                    queue.Enqueue(start);
                    bool supplied = false;

                    while (queue.Count > 0)
                    {
                        IntVec3 here = queue.Dequeue();
                        component.Add(here);
                        foreach (IntVec3 d in GenAdj.CardinalDirections)
                        {
                            IntVec3 next = here + d;
                            if (!next.InBounds(map)) continue;
                            if (IsCanal(next))
                            {
                                if (visited.Add(next))
                                    queue.Enqueue(next);
                            }
                            else if (!supplied && SourceAt(next))
                            {
                                supplied = true;
                            }
                        }
                    }

                    TerrainDef desired = supplied ?
                        AMJW_Defs.AMJW_DugCanalWet : AMJW_Defs.AMJW_DugCanalDry;
                    foreach (IntVec3 cell in component)
                    {
                        if (map.terrainGrid.TopTerrainAt(cell) != desired)
                            map.terrainGrid.SetTerrain(cell, desired);
                    }
                }
            }
            finally
            {
                changingCanalTerrain = false;
            }
            foreach (IntVec3 stale in staleCells)
                canalCells.Remove(stale);
        }
    }
}
