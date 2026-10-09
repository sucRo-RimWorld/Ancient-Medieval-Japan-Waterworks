using System;
using System.Threading.Tasks;
using AncientMedievalJapan.Waterworks;
using RimWorks.Pickle;
using RimWorld;
using Verse;

namespace AncientMedievalJapan.Waterworks.E2E
{
    /// <summary>
    /// Real disk save/load via Pickle's built-in "When I save and reload".
    /// Never retain a Map or MapComponent reference across that step.
    /// The surrounding Quickstart is isolated, so its disposable terrain
    /// fixture intentionally remains on the saved map.
    /// </summary>
    [PickleSteps]
    public sealed class WaterworksPersistenceSteps
    {
        private IntVec3 suppliedCell = IntVec3.Invalid;
        private IntVec3 dryCell = IntVec3.Invalid;
        private IntVec3 mudCell = IntVec3.Invalid;
        private IntVec3 marshCell = IntVec3.Invalid;
        private IntVec3 sourceCell = IntVec3.Invalid;

        [Given("Waterworks has supplied and dry canals with distinct original ground")]
        public Task Prepare(PickleContext ctx)
        {
            return GameThread.Run(delegate
            {
                // The test map belongs to this Quickstart scenario only.
                // Do NOT dispose the fixture before saving: that would
                // remove the canals that this test must persist.
                var fixture = new WaterworksSteps.Fixture(ctx);
                Map map = fixture.Map;
                CanalMapComponent component = fixture.Canal;
                suppliedCell = fixture.Cell(0, 0);
                dryCell = fixture.Cell(3, 0);
                sourceCell = fixture.Cell(-1, 0);
                mudCell = fixture.Cell(0, 3);
                marshCell = fixture.Cell(3, 3);

                map.terrainGrid.SetTerrain(suppliedCell, TerrainDefOf.Gravel);
                map.terrainGrid.SetTerrain(dryCell, TerrainDefOf.Soil);
                map.terrainGrid.SetTerrain(mudCell, DefDatabase<TerrainDef>.GetNamed("Mud"));
                map.terrainGrid.SetTerrain(marshCell, DefDatabase<TerrainDef>.GetNamed("Marsh"));
                map.terrainGrid.SetTerrain(sourceCell,
                    DefDatabase<TerrainDef>.GetNamed("WaterMovingShallow"));

                ctx.Require(component.Dig(suppliedCell), "Cannot excavate Gravel prior to save.");
                ctx.Require(component.Dig(dryCell), "Cannot excavate Soil prior to save.");
                ctx.Require(component.Dig(mudCell), "Cannot excavate Mud prior to save.");
                ctx.Require(component.Dig(marshCell), "Cannot excavate Marsh prior to save.");
                component.MapComponentTick();

                ctx.Assert(map.terrainGrid.TopTerrainAt(suppliedCell) ==
                    AMJW_Defs.AMJW_DugCanalWet, "Source-connected canal must be wet before saving.");
                ctx.Assert(map.terrainGrid.TopTerrainAt(dryCell) ==
                    AMJW_Defs.AMJW_DugCanalDry, "Unconnected canal must be dry before saving.");
                ctx.Assert(component.CanFill(suppliedCell).Accepted &&
                    component.CanFill(dryCell).Accepted &&
                    component.CanFill(mudCell).Accepted &&
                    component.CanFill(marshCell).Accepted,
                    "All four original terrain records must exist before saving.");
            });
        }

        // Between these steps Pickle executes its built-in:
        //    When I save and reload
        // That step writes a real .rws, loads a new Game and waits for
        // loaded maps to settle. Checking the new map is essential:
        // checking the same in-memory component would not test persistence.

        [Then("Waterworks rebuilds supply and restores both saved ground types")]
        public Task VerifyAfterReload(PickleContext ctx)
        {
            return GameThread.Run(delegate
            {
                Map loadedMap = Find.CurrentMap;
                ctx.Require(loadedMap != null, "No map after real save/reload.");
                ctx.Require(suppliedCell.IsValid && dryCell.IsValid && sourceCell.IsValid &&
                    mudCell.IsValid && marshCell.IsValid,
                    "Persistence setup coordinates did not survive the scenario.");
                CanalMapComponent loaded = loadedMap.GetComponent<CanalMapComponent>();
                ctx.Require(loaded != null, "Waterworks component missing after reload.");

                ctx.Assert(loadedMap.terrainGrid.TopTerrainAt(suppliedCell) ==
                    AMJW_Defs.AMJW_DugCanalWet,
                    "Connected canal must be wet again after reload.");
                ctx.Assert(loadedMap.terrainGrid.TopTerrainAt(dryCell) ==
                    AMJW_Defs.AMJW_DugCanalDry,
                    "Disconnected canal must remain dry after reload.");
                ctx.Assert(loaded.CanFill(suppliedCell).Accepted &&
                    loaded.CanFill(dryCell).Accepted &&
                    loaded.CanFill(mudCell).Accepted &&
                    loaded.CanFill(marshCell).Accepted,
                    "Original-terrain records including wetlands were not deserialized.");
                ctx.Assert(loadedMap.terrainGrid.TopTerrainAt(mudCell) ==
                    AMJW_Defs.AMJW_DugCanalDry &&
                    loadedMap.terrainGrid.TopTerrainAt(marshCell) ==
                    AMJW_Defs.AMJW_DugCanalDry,
                    "Unconnected wetland canals must remain dry after reload.");

                // Test the terrain-change event subscription after loading.
                loadedMap.terrainGrid.SetTerrain(sourceCell, TerrainDefOf.Soil);
                loaded.MapComponentTick();
                ctx.Assert(loadedMap.terrainGrid.TopTerrainAt(suppliedCell) ==
                    AMJW_Defs.AMJW_DugCanalDry,
                    "Loaded map failed to dry a canal after source disconnection.");
                loadedMap.terrainGrid.SetTerrain(sourceCell,
                    DefDatabase<TerrainDef>.GetNamed("WaterMovingShallow"));
                loaded.MapComponentTick();
                ctx.Assert(loadedMap.terrainGrid.TopTerrainAt(suppliedCell) ==
                    AMJW_Defs.AMJW_DugCanalWet,
                    "Loaded map failed to rewet a canal after source reconnection.");

                ctx.Require(loaded.Fill(suppliedCell), "Could not fill loaded supplied canal.");
                ctx.Require(loaded.Fill(dryCell), "Could not fill loaded dry canal.");
                ctx.Require(loaded.Fill(mudCell), "Could not fill loaded Mud canal.");
                ctx.Require(loaded.Fill(marshCell), "Could not fill loaded Marsh canal.");
                ctx.Assert(loadedMap.terrainGrid.TopTerrainAt(suppliedCell) ==
                    TerrainDefOf.Gravel,
                    "Loaded original Gravel not restored by Fill.");
                ctx.Assert(loadedMap.terrainGrid.TopTerrainAt(dryCell) ==
                    TerrainDefOf.Soil,
                    "Loaded original Soil not restored by Fill.");
                ctx.Assert(loadedMap.terrainGrid.TopTerrainAt(mudCell).defName == "Mud",
                    "Loaded original Mud not restored by Fill.");
                ctx.Assert(loadedMap.terrainGrid.TopTerrainAt(marshCell).defName == "Marsh",
                    "Loaded original Marsh not restored by Fill.");
            });
        }
    }
}
