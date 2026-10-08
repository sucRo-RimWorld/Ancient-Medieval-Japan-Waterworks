using System.Collections.Generic;
using RimWorks.Pickle;
using RimWorld;
using Verse;

namespace AncientMedievalJapan.Waterworks.E2E
{
    /// <summary>A baseline save must contain no Waterworks Defs or map state.</summary>
    [PickleSteps]
    public sealed class WaterworksBootstrapSteps
    {
        [Then("the Vanilla save baseline is prepared without Waterworks")]
        public void PrepareVanilla(PickleContext ctx)
        {
            Map map = Find.CurrentMap;
            ctx.Require(map != null && map.Size.x >= 30 && map.Size.z >= 30,
                "Expected a vanilla Quickstart game map.");
            ctx.Require(DefDatabase<TerrainDef>.GetNamedSilentFail("AMJW_DugCanalDry") == null &&
                DefDatabase<TerrainDef>.GetNamedSilentFail("AMJW_DugCanalWet") == null,
                "Waterworks was loaded into the pre-install baseline!");

            // Stable coordinates are used across the separate, cold-started test
            // process. This is a scratch Quickstart, never a user's real save.
            // Normalize a buffer around the targets so generated natural
            // rivers/ponds cannot unexpectedly wet the distant Gravel canal.
            // This is a disposable test map, not a player's existing colony.
            for (int x = 9; x <= 17; x++)
                for (int z = 9; z <= 15; z++)
                {
                    IntVec3 cell = new IntVec3(x, 0, z);
                    map.fogGrid.Unfog(cell);
                    foreach (Thing thing in new List<Thing>(cell.GetThingList(map)))
                    {
                        if (thing.def.category == ThingCategory.Building || thing is Plant)
                            thing.Destroy(DestroyMode.Vanish);
                    }
                    ctx.Require(map.terrainGrid.FoundationAt(cell) == null &&
                        map.terrainGrid.UnderTerrainAt(cell) == null,
                        "Baseline test region contains layered floor or foundation.");
                    map.terrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
                }
            IntVec3[] cells = {
                new IntVec3(11, 0, 12),
                new IntVec3(12, 0, 12),
                new IntVec3(15, 0, 12)
            };
            foreach (IntVec3 cell in cells)
            {
                map.fogGrid.Unfog(cell);
                foreach (Thing thing in new List<Thing>(cell.GetThingList(map)))
                {
                    if (thing.def.category == ThingCategory.Building || thing is Plant)
                        thing.Destroy(DestroyMode.Vanish);
                }
                ctx.Require(map.terrainGrid.FoundationAt(cell) == null &&
                    map.terrainGrid.UnderTerrainAt(cell) == null,
                    "Baseline test region contains existing foundation or layered floor.");
            }
            map.terrainGrid.SetTerrain(cells[0],
                DefDatabase<TerrainDef>.GetNamed("WaterMovingShallow"));
            map.terrainGrid.SetTerrain(cells[1], TerrainDefOf.Soil);
            map.terrainGrid.SetTerrain(cells[2], TerrainDefOf.Gravel);
            ctx.Assert(map.terrainGrid.TopTerrainAt(cells[1]) == TerrainDefOf.Soil &&
                map.terrainGrid.TopTerrainAt(cells[2]) == TerrainDefOf.Gravel,
                "Baseline terrain did not initialize.");
        }
    }
}
