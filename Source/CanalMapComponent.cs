using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AncientMedievalJapan.Waterworks
{
    /// <summary>Binary, four-direction canal supply and original terrain restoration.</summary>
    public sealed class CanalMapComponent : MapComponent
    {
        private Dictionary<int, string> originals = new Dictionary<int, string>();
        private readonly HashSet<IntVec3> visited = new HashSet<IntVec3>();
        private readonly Queue<IntVec3> queue = new Queue<IntVec3>();
        private readonly List<IntVec3> component = new List<IntVec3>();

        public CanalMapComponent(Map map) : base(map) { }

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
            Recalculate();
        }

        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame % 2500 == 0) Recalculate();
        }

        public bool IsCanal(IntVec3 c)
        {
            if (!c.InBounds(map)) return false;
            TerrainDef t = map.terrainGrid.TopTerrainAt(c);
            return t == AMJW_Defs.AMJW_DugCanalDry || t == AMJW_Defs.AMJW_DugCanalWet;
        }

        public AcceptanceReport CanDig(IntVec3 c)
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
                if (thing is Plant || thing.def.category == ThingCategory.Building)
                    return "AMJW_Blocked".Translate();
            }
            if (t == TerrainDefOf.Ice || t.IsWater || t.defName == "Marsh" ||
                t.defName == "Mud" || t.affordances == null ||
                !t.affordances.Contains(TerrainAffordanceDefOf.Diggable))
                return "AMJW_CannotDig".Translate();
            return AcceptanceReport.WasAccepted;
        }

        public AcceptanceReport CanFill(IntVec3 c)
        {
            if (!c.InBounds(map) || c.Fogged(map)) return false;
            if (!IsCanal(c)) return "AMJW_NotCanal".Translate();
            if (map.terrainGrid.FoundationAt(c) != null ||
                map.terrainGrid.UnderTerrainAt(c) != null ||
                c.GetFirstBuilding(map) != null) return "AMJW_Blocked".Translate();
            string oldDefName;
            if (!originals.TryGetValue(map.cellIndices.CellToIndex(c), out oldDefName) ||
                DefDatabase<TerrainDef>.GetNamedSilentFail(oldDefName) == null)
                return "AMJW_CannotRestore".Translate();
            return AcceptanceReport.WasAccepted;
        }

        public bool Dig(IntVec3 c)
        {
            if (!CanDig(c).Accepted) return false;
            originals[map.cellIndices.CellToIndex(c)] = map.terrainGrid.TopTerrainAt(c).defName;
            map.terrainGrid.SetTerrain(c, AMJW_Defs.AMJW_DugCanalDry);
            Recalculate();
            return true;
        }

        public bool Fill(IntVec3 c)
        {
            if (!CanFill(c).Accepted) return false;
            int index = map.cellIndices.CellToIndex(c);
            TerrainDef original = DefDatabase<TerrainDef>.GetNamedSilentFail(originals[index]);
            if (original == null) return false;
            map.terrainGrid.SetTerrain(c, original);
            originals.Remove(index);
            Recalculate();
            return true;
        }

        private static bool IsStanding(TerrainDef t)
            => t.defName == "WaterShallow" || t.defName == "WaterDeep";

        private bool SourceAt(IntVec3 c)
        {
            if (!c.InBounds(map)) return false;
            TerrainDef t = map.terrainGrid.TopTerrainAt(c);
            if (t.defName == "WaterMovingShallow" || t.defName == "WaterMovingChestDeep")
                return true;
            return IsStanding(t) && StandingBodyHasNineCells(c);
        }

        private bool StandingBodyHasNineCells(IntVec3 start)
        {
            // Bounded BFS; do not traverse a whole lake once nine cells are found.
            HashSet<IntVec3> seen = new HashSet<IntVec3> { start };
            Queue<IntVec3> pending = new Queue<IntVec3>();
            pending.Enqueue(start);
            while (pending.Count != 0)
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
            visited.Clear();
            for (int index = 0; index < map.cellIndices.NumGridCells; index++)
            {
                IntVec3 start = map.cellIndices.IndexToCell(index);
                if (!IsCanal(start) || !visited.Add(start)) continue;
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
                            if (visited.Add(next)) queue.Enqueue(next);
                        }
                        else if (!supplied && SourceAt(next))
                        {
                            supplied = true;
                        }
                    }
                }
                TerrainDef desired = supplied ? AMJW_Defs.AMJW_DugCanalWet :
                                              AMJW_Defs.AMJW_DugCanalDry;
                foreach (IntVec3 cell in component)
                {
                    if (map.terrainGrid.TopTerrainAt(cell) != desired)
                        map.terrainGrid.SetTerrain(cell, desired);
                }
            }
        }
    }
}