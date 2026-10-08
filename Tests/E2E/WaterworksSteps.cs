using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AncientMedievalJapan.Waterworks;
using RimWorks.Pickle;
using RimWorld;
using Verse;

namespace AncientMedievalJapan.Waterworks.E2E
{
    internal static class GameThread
    {
        public static Task Run(Action check)
        {
            var result = new TaskCompletionSource<bool>();
            RimWorks.Pickle.Runtime.PickleDriver.Post(delegate
            {
                try
                {
                    if (!UnityData.IsInMainThread)
                        throw new InvalidOperationException("Pickle test off main thread");
                    check();
                    result.SetResult(true);
                }
                catch (Exception ex) { result.SetException(ex); }
            });
            return result.Task;
        }
    }

    [PickleSteps]
    public sealed class WaterworksSteps
    {
        [Then("Waterworks loaded Defs preserve the canal contract")]
        public Task Loaded(PickleContext c)
        {
            return GameThread.Run(delegate
            {
                TerrainDef dry = DefDatabase<TerrainDef>.GetNamed("AMJW_DugCanalDry");
                TerrainDef wet = DefDatabase<TerrainDef>.GetNamed("AMJW_DugCanalWet");
                c.Require(dry != null && wet != null, "Missing canal terrain");
                c.Assert(dry.pathCost == 10 && wet.pathCost == 10, "Canal path cost");
                c.Assert(dry.affordances.Contains(TerrainAffordanceDefOf.Bridgeable) &&
                    wet.affordances.Contains(TerrainAffordanceDefOf.Bridgeable), "Bridgeable");
                c.Assert(!wet.IsWater && !wet.IsRiver, "Canal mistaken for natural water");
                c.Require(DefDatabase<DesignationDef>.GetNamed("AMJW_DigCanal") != null, "Dig designation");
                c.Require(DefDatabase<DesignationDef>.GetNamed("AMJW_FillCanal") != null, "Fill designation");
                c.Require(DefDatabase<JobDef>.GetNamed("AMJW_DigCanalJob") != null, "Dig JobDef");
                c.Require(DefDatabase<JobDef>.GetNamed("AMJW_FillCanalJob") != null, "Fill JobDef");
                c.Require(TerrainDefOf.Bridge != null && TerrainDefOf.Bridge.isFoundation,
                    "Bridge foundation not resolved");
            });
        }

        [Then("cardinal canal branches connect disconnect and reconnect")]
        public Task Graph(PickleContext c)
        {
            return GameThread.Run(delegate
            {
                using (var f = new Fixture(c))
                {
                    IntVec3 a = f.Cell(0, 0), b = f.Cell(1, 0);
                    IntVec3 end = f.Cell(2, 0), branch = f.Cell(1, 1);
                    c.Require(f.Canal.Dig(a) && f.Canal.Dig(b) &&
                        f.Canal.Dig(end) && f.Canal.Dig(branch), "Failed to dig network");
                    f.Dry(end, "unsupplied branch");
                    f.Set(-1, 1, "WaterMovingShallow"); f.Tick();
                    f.Dry(a, "diagonal source should not supply");
                    f.Set(-1, 0, "WaterMovingShallow"); f.Tick();
                    f.Wet(a, "source side"); f.Wet(end, "end");
                    f.Wet(branch, "T branch");
                    c.Require(f.Canal.Fill(b), "Could not sever the connection");
                    f.Wet(a, "source side after severing");
                    f.Dry(end, "disconnected end"); f.Dry(branch, "disconnected branch");
                    c.Require(f.Canal.Dig(b), "Could not reconnect the canal");
                    f.Wet(end, "reconnected end");
                }
            });
        }

        [Then("standing ponds use the nine cell freshwater threshold")]
        public Task Standing(PickleContext c)
        {
            return GameThread.Run(delegate
            {
                using (var f = new Fixture(c))
                {
                    IntVec3 a = f.Cell(0, 0);
                    c.Require(f.Canal.Dig(a), "Cannot dig pond tester");
                    f.Set(-1, 0, "WaterOceanShallow"); f.Tick();
                    f.Dry(a, "ocean must not supply");
                    f.Set(-1, 0, "Marsh"); f.Tick();
                    f.Dry(a, "marsh must not supply");
                    for (int x = -3; x <= -1; x++)
                        for (int z = -1; z <= 1; z++)
                            if (x != -3 || z != 1) f.Set(x, z, "WaterShallow");
                    f.Tick(); f.Dry(a, "8-cell pond");
                    f.Set(-3, 1, "WaterDeep"); f.Tick();
                    f.Wet(a, "9-cell mixed-depth pond");
                    f.Set(-3, 1, "Soil"); f.Tick();
                    f.Dry(a, "pond shrank to 8");
                }
            });
        }

        [Then("Vanilla bridge preserves water and gravel restoration")]
        public Task Bridge(PickleContext c)
        {
            return GameThread.Run(delegate
            {
                using (var f = new Fixture(c))
                {
                    IntVec3 a = f.Cell(0, 0);
                    f.Set(0, 0, "Gravel");
                    c.Require(f.Canal.Dig(a), "Cannot dig Gravel");
                    f.Set(-1, 0, "WaterMovingChestDeep"); f.Tick();
                    f.Wet(a, "moving freshwater");
                    f.Map.terrainGrid.SetFoundation(a, TerrainDefOf.Bridge);
                    c.Assert(f.Map.terrainGrid.FoundationAt(a) == TerrainDefOf.Bridge, "Bridge missing");
                    c.Assert(f.Canal.IsCanal(a), "Bridge erased canal");
                    c.Assert(!f.Canal.CanFill(a).Accepted, "Fill allowed under bridge");
                    f.Set(-1, 0, "Soil"); f.Tick(); f.Dry(a, "source cut under bridge");
                    f.Set(-1, 0, "WaterMovingShallow"); f.Tick(); f.Wet(a, "source restored");
                    f.Map.terrainGrid.RemoveFoundation(a, false);
                    c.Require(f.Canal.Fill(a), "Cannot fill after bridge removal");
                    c.Assert(f.Map.terrainGrid.TopTerrainAt(a).defName == "Gravel",
                        "Original Gravel not restored");
                }
            });
        }

        private sealed class Fixture : IDisposable
        {
            private readonly PickleContext ctx;
            private readonly IntVec3 origin;
            private readonly Dictionary<IntVec3, TerrainDef> originals =
                new Dictionary<IntVec3, TerrainDef>();
            public readonly Map Map;
            public readonly CanalMapComponent Canal;
            public Fixture(PickleContext context)
            {
                ctx = context;
                Map = Find.CurrentMap;
                ctx.Require(Map != null && Map.Size.x >= 40 && Map.Size.z >= 40,
                    "Expected 50x50 Quickstart map");
                Canal = Map.GetComponent<CanalMapComponent>();
                ctx.Require(Canal != null, "Waterworks component missing");
                bool found = false;
                IntVec3 chosen = IntVec3.Invalid;
                for (int x = 8; x < Map.Size.x - 8 && !found; x++)
                    for (int z = 8; z < Map.Size.z - 8 && !found; z++)
                    {
                        IntVec3 p = new IntVec3(x, 0, z);
                        bool valid = true;
                        for (int dx = -5; dx <= 5 && valid; dx++)
                            for (int dz = -5; dz <= 5; dz++)
                            {
                                IntVec3 at = p + new IntVec3(dx, 0, dz);
                                if (at.Fogged(Map) || at.GetFirstBuilding(Map) != null ||
                                    Map.terrainGrid.UnderTerrainAt(at) != null ||
                                    Map.terrainGrid.FoundationAt(at) != null)
                                { valid = false; break; }
                            }
                        if (valid) { found = true; chosen = p; }
                    }
                ctx.Require(found, "No unobstructed revealed 11x11 test patch");
                origin = chosen;
                for (int dx = -5; dx <= 5; dx++)
                    for (int dz = -5; dz <= 5; dz++)
                    {
                        IntVec3 cell = Cell(dx, dz);
                        originals[cell] = Map.terrainGrid.TopTerrainAt(cell);
                        foreach (Thing t in new List<Thing>(cell.GetThingList(Map)))
                            if (t is Plant) t.Destroy(DestroyMode.Vanish);
                        Map.terrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
                    }
                Canal.Recalculate();
            }
            public IntVec3 Cell(int x, int z) { return origin + new IntVec3(x, 0, z); }
            public void Set(int x, int z, string defName)
            {
                Map.terrainGrid.SetTerrain(Cell(x, z), DefDatabase<TerrainDef>.GetNamed(defName));
            }
            public void Tick() { Canal.MapComponentTick(); }
            public void Wet(IntVec3 c, string why)
            {
                ctx.Assert(Map.terrainGrid.TopTerrainAt(c).defName == "AMJW_DugCanalWet", why);
            }
            public void Dry(IntVec3 c, string why)
            {
                ctx.Assert(Map.terrainGrid.TopTerrainAt(c).defName == "AMJW_DugCanalDry", why);
            }
            public void Dispose()
            {
                foreach (KeyValuePair<IntVec3, TerrainDef> item in originals)
                {
                    if (Map.terrainGrid.FoundationAt(item.Key) != null)
                        Map.terrainGrid.RemoveFoundation(item.Key, false);
                    Map.terrainGrid.SetTerrain(item.Key, item.Value);
                }
                Canal.Recalculate();
            }
        }
    }
}
