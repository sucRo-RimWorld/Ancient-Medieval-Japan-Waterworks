using System;
using System.Threading.Tasks;
using AncientMedievalJapan.Waterworks;
using RimWorks.Pickle;
using RimWorld;
using Verse;

namespace AncientMedievalJapan.Waterworks.E2E
{
    /// <summary>
    /// Phase B: load the game created in Phase A without the Waterworks mod.
    /// The runner performs a fresh process start with Waterworks enabled.
    /// </summary>
    [PickleSteps]
    public sealed class WaterworksAddToSaveSteps
    {
        private static readonly IntVec3 source = new IntVec3(11, 0, 12);
        private static readonly IntVec3 soil = new IntVec3(12, 0, 12);
        private static readonly IntVec3 gravel = new IntVec3(15, 0, 12);

        [Then("Waterworks first loads without changing Vanilla ground and can dig canals")]
        public Task InstallAndDig(PickleContext ctx)
        {
            return GameThread.Run(delegate
            {
                Map map = Find.CurrentMap;
                ctx.Require(map != null, "Vanilla saved map not loaded.");
                CanalMapComponent canal = map.GetComponent<CanalMapComponent>();
                ctx.Require(canal != null, "Waterworks MapComponent not attached to old map.");
                ctx.Assert(map.terrainGrid.TopTerrainAt(source).defName == "WaterMovingShallow",
                    "Original freshwater source altered on first modded load.");
                ctx.Assert(map.terrainGrid.TopTerrainAt(soil) == TerrainDefOf.Soil &&
                    map.terrainGrid.TopTerrainAt(gravel) == TerrainDefOf.Gravel,
                    "Existing Vanilla soil/gravel altered on mod installation.");
                ctx.Assert(!canal.IsCanal(soil) && !canal.IsCanal(gravel),
                    "Waterworks created unrequested canals when loading the old save.");
                ctx.Require(canal.Dig(soil), "Cannot dig Vanilla Soil on old saved map.");
                ctx.Require(canal.Dig(gravel), "Cannot dig Vanilla Gravel on old saved map.");
                ctx.Assert(map.terrainGrid.TopTerrainAt(soil) == AMJW_Defs.AMJW_DugCanalWet,
                    "Connected Soil canal not wet after fresh installation.");
                ctx.Assert(map.terrainGrid.TopTerrainAt(gravel) == AMJW_Defs.AMJW_DugCanalDry,
                    "Isolated Gravel canal not dry after fresh installation.");
            });
        }

        [Then("Waterworks persists and restores old Vanilla soil and gravel")]
        public Task AfterSaveReload(PickleContext ctx)
        {
            return GameThread.Run(delegate
            {
                Map map = Find.CurrentMap;
                ctx.Require(map != null, "Modded saved map not reloaded.");
                CanalMapComponent canal = map.GetComponent<CanalMapComponent>();
                ctx.Require(canal != null, "Waterworks MapComponent missing after second load.");
                ctx.Assert(map.terrainGrid.TopTerrainAt(soil) == AMJW_Defs.AMJW_DugCanalWet,
                    "Newly installed mod's supplied canal was not persisted.");
                ctx.Assert(map.terrainGrid.TopTerrainAt(gravel) == AMJW_Defs.AMJW_DugCanalDry,
                    "Newly installed mod's dry canal was not persisted.");
                ctx.Require(canal.CanFill(soil).Accepted && canal.CanFill(gravel).Accepted,
                    "Original-terrain records lost across upgraded save round trip.");
                ctx.Require(canal.Fill(soil) && canal.Fill(gravel),
                    "Cannot fill two canals on the upgraded saved map.");
                ctx.Assert(map.terrainGrid.TopTerrainAt(soil) == TerrainDefOf.Soil &&
                    map.terrainGrid.TopTerrainAt(gravel) == TerrainDefOf.Gravel,
                    "Loaded canals failed to restore the distinct original Vanilla grounds.");
            });
        }
    }
}
