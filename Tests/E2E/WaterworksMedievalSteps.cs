using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RimWorks.Pickle;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace AncientMedievalJapan.Waterworks.E2E
{
    [PickleSteps]
    public sealed class WaterworksMedievalSteps
    {
        private IntVec3 gateCell, sourceCell;
        private static Type Type(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);
        }
        private static MethodInfo Method(Type type, string name)
        {
            return type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        }
        private static FieldInfo Field(Type type, string name)
        {
            for (; type != null; type = type.BaseType)
            {
                var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (field != null) return field;
            }
            throw new InvalidOperationException("Missing audited field " + name);
        }
        private static ThingComp Sluice(Map map, IntVec3 at)
        {
            return at.GetThingList(map).OfType<ThingWithComps>().SelectMany(t => t.AllComps)
                .First(c => c.GetType().FullName == "DBHforMedieval.CompSluiceGate");
        }
        private static List<IntVec3> Cells(ThingComp comp)
        {
            return ((IEnumerable<IntVec3>)Method(comp.GetType(), "ActiveCells").Invoke(comp, null)).ToList();
        }
        private static object Grid(Map map)
        {
            return Method(Type("DBHforMedieval.IrrigationUtility"), "Grid").Invoke(null, new object[] { map });
        }
        private static float Moisture(Map map, IntVec3 cell)
        {
            return (float)Method(Type("DBHforMedieval.IrrigationUtility"), "GetMoistureAt")
                .Invoke(null, new object[] { map, cell });
        }
        private static void Dry(Map map, IEnumerable<IntVec3> cells)
        {
            object grid = Grid(map);
            MethodInfo set = Method(grid.GetType(), "SetAt");
            foreach (var cell in cells) set.Invoke(grid, new object[] { cell, 0f, true, false, MapMeshFlagDefOf.Terrain });
        }
        private static bool Spray(ThingComp comp)
        {
            return (bool)Method(comp.GetType(), "TrySpray").Invoke(comp, null);
        }
        private static void VerifySpray(PickleContext ctx, ThingComp comp, bool expected)
        {
            var cells = Cells(comp);
            ctx.Require(cells.Count > 0, "Native sluice has no active cells");
            Dry(comp.parent.Map, cells);
            ctx.Assert(Spray(comp) == expected, "Native TrySpray result");
            foreach (var cell in cells)
                ctx.Assert(Math.Abs(Moisture(comp.parent.Map, cell) - (expected ? 5f : 0f)) < 0.001f,
                    "Native irrigation-grid delta must be " + (expected ? "5" : "0") + " at " + cell);
        }
        private ThingComp Prepare(PickleContext ctx, WaterworksSteps.Fixture f)
        {
            sourceCell = f.Cell(-1, 0);
            ctx.Require(f.Canal.Dig(f.Cell(0, 0)), "Waterworks intake");
            f.Set(-1, 0, "WaterMovingShallow");
            foreach (var cell in new[] { f.Cell(1, 0), f.Cell(2, 0), f.Cell(2, 1) })
            {
                var canal = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("ES_IrrigationCanal"), ThingDefOf.WoodLog);
                canal.SetFaction(Faction.OfPlayer);
                GenSpawn.Spawn(canal, cell, f.Map);
            }
            gateCell = f.Cell(2, 1);
            var gate = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("ES_SluiceGate"), ThingDefOf.WoodLog);
            gate.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(gate, gateCell, f.Map, Rot4.North);
            var comp = Sluice(f.Map, gateCell);
            Field(comp.GetType(), "length").SetValue(comp, 3);
            Method(comp.GetType(), "UpdateRectangle").Invoke(comp, null);
            f.Tick();
            ctx.Require(Cells(comp).Count > 0, "Sluice native rectangle");
            return comp;
        }
        [Then("Medieval sluice irrigates from natural Waterworks supply and tracks disconnection")]
        public Task Natural(PickleContext ctx)
        {
            return GameThread.Run(delegate
            {
                if (System.Environment.GetEnvironmentVariable("AMJW_EXPECT_NO_NEEDS") == "1")
                {
                    var settingsType = Type("DubsBadHygiene.Settings");
                    object settings = Field(Type("DubsBadHygiene.DubsBadHygieneMod"), "Settings").GetValue(null);
                    ctx.Assert(!(bool)Field(settingsType, "BladderNeed").GetValue(settings) &&
                        !(bool)Field(settingsType, "HygieneNeed").GetValue(settings) &&
                        !(bool)settingsType.GetProperty("ThirstNeed").GetValue(null, null) &&
                        !(bool)settingsType.GetProperty("LiteMode").GetValue(null, null),
                        "Actual DBH needs must be OFF while water management remains ON");
                }
                using (var f = new WaterworksSteps.Fixture(ctx))
                {
                    var comp = Prepare(ctx, f);
                    ctx.Assert((bool)Method(comp.GetType(), "IsWaitingForDoer").Invoke(comp, null), "Dry supplied sluice offers native work");
                    VerifySpray(ctx, comp, true);
                    // Multiple contacts and a bridge must not multiply irrigation.
                    ctx.Require(f.Canal.Dig(f.Cell(0, 1)), "Second natural endpoint");
                    f.Map.terrainGrid.SetFoundation(f.Cell(0, 0), TerrainDefOf.Bridge);
                    VerifySpray(ctx, comp, true);
                    f.Set(-1, 0, "Soil");
                    VerifySpray(ctx, comp, false);
                    ctx.Assert(!(bool)Method(comp.GetType(), "IsWaitingForDoer").Invoke(comp, null), "Disconnected sluice must stop offering work");
                    f.Set(-1, 0, "WaterMovingShallow");
                    VerifySpray(ctx, comp, true);
                }
            });
        }
        [Then("a pawn opens the native Medieval sluice and its ticks irrigate soil")]
        public async Task PawnWork(PickleContext ctx)
        {
            WaterworksSteps.Fixture f = null;
            Pawn worker = null;
            ThingComp comp = null;
            TimeSpeed speed = TimeSpeed.Normal;
            bool irrigated = false;
            try
            {
                await GameThread.Run(delegate
                {
                    speed = Find.TickManager.CurTimeSpeed;
                    Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                    f = new WaterworksSteps.Fixture(ctx);
                    comp = Prepare(ctx, f);
                    Dry(f.Map, Cells(comp));
                    for (int i = 0; i < 30; i++)
                    {
                        var pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
                        if (!pawn.Downed && !pawn.WorkTypeIsDisabled(WorkTypeDefOf.Growing)) { worker = pawn; break; }
                        pawn.Destroy();
                    }
                    ctx.Require(worker != null, "No capable grower");
                    GenSpawn.Spawn(worker, f.Cell(3, 1), f.Map);
                    worker.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
                    if (worker.needs.food != null) worker.needs.food.CurLevelPercentage = 1f;
                    if (worker.needs.rest != null) worker.needs.rest.CurLevelPercentage = 1f;
                    var giver = (WorkGiver_Scanner)DefDatabase<WorkGiverDef>.GetNamed("ES_OpenSluiceGate").Worker;
                    ctx.Require(!giver.ShouldSkip(worker, true) && giver.HasJobOnThing(worker, comp.parent, true), "Native workgiver refuses supplied sluice");
                    var job = giver.JobOnThing(worker, comp.parent, true);
                    ctx.Require(job != null && job.def.defName == "ES_OpenSluiceGate", "Native sluice job missing");
                    job.playerForced = true;
                    worker.jobs.StartJob(job, JobCondition.InterruptForced);
                    ctx.Require(worker.CurJob == job, "Native sluice job did not start");
                });
                for (int batch = 0; batch < 40 && !irrigated; batch++)
                {
                    await GameThread.Run(delegate
                    {
                        for (int i = 0; i < 32; i++)
                        {
                            Find.TickManager.DoSingleTick();
                            if (Cells(comp).All(cell => Moisture(f.Map, cell) >= 5f)) { irrigated = true; break; }
                        }
                    });
                    if (!irrigated) await Task.Delay(1);
                }
                ctx.Require(irrigated, "Native pawn/JobDriver/CompTick did not irrigate within 1280 ticks");
            }
            finally
            {
                await GameThread.Run(delegate
                {
                    if (worker != null) { if (worker.CurJob != null) worker.jobs.EndCurrentJob(JobCondition.InterruptForced, false); worker.Destroy(); }
                    if (f != null) f.Dispose();
                    Find.TickManager.CurTimeSpeed = speed;
                });
            }
        }
        [Then("Medieval sluice rejects diagonal only and severed foreign canal chains")]
        public Task Topology(PickleContext ctx)
        {
            return GameThread.Run(delegate
            {
                using (var f = new WaterworksSteps.Fixture(ctx))
                {
                    var comp = Prepare(ctx, f);
                    f.Cell(1, 0).GetThingList(f.Map).First(t => t.def.defName == "ES_IrrigationCanal").Destroy();
                    VerifySpray(ctx, comp, false);
                    // The remaining bend at (2,0) touches only diagonally.
                    ctx.Require(f.Canal.Dig(f.Cell(1, -1)), "Diagonal-only canal");
                    ctx.Require(f.Canal.Dig(f.Cell(0, -1)), "Supply diagonal branch");
                    VerifySpray(ctx, comp, false);
                }
            });
        }
        [Then("native DBH tower supply takes precedence and remains after source loss")]
        public Task Storage(PickleContext ctx)
        {
            return GameThread.Run(delegate
            {
                using (var f = new WaterworksSteps.Fixture(ctx))
                {
                    var comp = Prepare(ctx, f);
                    object pipe = Field(comp.GetType(), "PipeComp").GetValue(comp);
                    object net = pipe.GetType().GetProperty("pipeNet").GetValue(pipe, null);
                    ctx.Require(net != null, "Actual DBH network missing");
                    var towers = (IList)Field(net.GetType(), "WaterTowers").GetValue(net);
                    ctx.Require(towers.Count == 0, "Fixture unexpectedly has native towers");
                    // Real spawned storage Comp, registered to the real partner net for a bounded native-consumption control.
                    var towerDef = DefDatabase<ThingDef>.AllDefs.First(d => d.comps != null && d.comps.Any(p => p.compClass.FullName == "DubsBadHygiene.CompWaterStorage"));
                    var tower = (ThingWithComps)ThingMaker.MakeThing(towerDef, towerDef.MadeFromStuff ? ThingDefOf.WoodLog : null);
                    GenSpawn.Spawn(tower, f.Cell(-4, -4), f.Map);
                    var storage = tower.AllComps.First(c => c.GetType().FullName == "DubsBadHygiene.CompWaterStorage");
                    var water = Field(storage.GetType(), "WaterStorage");
                    try
                    {
                        towers.Add(storage);
                        water.SetValue(storage, 100f);
                        float usage = 9f * (float)Field(comp.props.GetType(), "usagePerCell").GetValue(comp.props);
                        VerifySpray(ctx, comp, true);
                        ctx.Assert(Math.Abs((float)water.GetValue(storage) - (100f - usage)) < 0.001f, "Native storage was not charged exactly once");
                        f.Set(-1, 0, "Soil");
                        VerifySpray(ctx, comp, true);
                        ctx.Assert(Math.Abs((float)water.GetValue(storage) - (100f - 2f * usage)) < 0.001f, "Natural disconnection affected native supply");
                        f.Set(-1, 0, "WaterMovingShallow");
                        water.SetValue(storage, 0.1f);
                        VerifySpray(ctx, comp, true);
                        ctx.Assert(Math.Abs((float)water.GetValue(storage) - 0.1f) < 0.001f, "Natural fallback drained insufficient storage");
                    }
                    finally { towers.Remove(storage); tower.Destroy(); }
                }
            });
        }
        [Given("Medieval irrigation and Waterworks topology exist before a disk save")]
        public Task SavePrepare(PickleContext ctx)
        {
            return GameThread.Run(delegate { var f = new WaterworksSteps.Fixture(ctx); VerifySpray(ctx, Prepare(ctx, f), true); });
        }
        [Then("reloaded Medieval sluice irrigates and source loss still stops it")]
        public Task Reload(PickleContext ctx)
        {
            return GameThread.Run(delegate
            {
                var map = Find.CurrentMap;
                var comp = Sluice(map, gateCell);
                VerifySpray(ctx, comp, true);
                map.terrainGrid.SetTerrain(sourceCell, TerrainDefOf.Soil);
                VerifySpray(ctx, comp, false);
                map.terrainGrid.SetTerrain(sourceCell, DefDatabase<TerrainDef>.GetNamed("WaterMovingShallow"));
                VerifySpray(ctx, comp, true);
            });
        }
        private static bool ClearPatch(Map map, IntVec3 center)
        {
            for (int x = -5; x <= 5; x++)
                for (int z = -5; z <= 5; z++)
                {
                    var at = center + new IntVec3(x, 0, z);
                    if (!at.InBounds(map) || at.Fogged(map) || at.GetFirstBuilding(map) != null ||
                        map.terrainGrid.UnderTerrainAt(at) != null || map.terrainGrid.FoundationAt(at) != null)
                        return false;
                }
            return true;
        }
        [Then("Medieval supply is map local and survives removal of another supplied map")]
        public Task MapLifecycle(PickleContext ctx)
        {
            return GameThread.Run(delegate
            {
                Map retained = Find.CurrentMap;
                Map removed = null;
                MapParent parent = null;
                WaterworksSteps.Fixture first = null;
                TimeSpeed speed = Find.TickManager.CurTimeSpeed;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                try
                {
                    ctx.Require(retained != null && Find.Maps.Contains(retained), "Primary real map missing");
                    PlanetTile tile = PlanetTile.Invalid;
                    for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
                    {
                        var candidate = new PlanetTile(i);
                        if (Find.WorldGrid[candidate].PrimaryBiome == retained.Biome &&
                            !Find.WorldObjects.AllWorldObjects.Any(w => w.Tile == candidate))
                        { tile = candidate; break; }
                    }
                    ctx.Require(tile.Valid, "No separate unused land tile for the real second map");
                    parent = (MapParent)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    parent.Tile = tile;
                    parent.SetFaction(Faction.OfPlayer);
                    Find.WorldObjects.Add(parent);
                    removed = MapGenerator.GenerateMap(new IntVec3(50, 1, 50), parent,
                        DefDatabase<MapGeneratorDef>.GetNamed("Base_Player"));
                    ctx.Require(removed != retained && Find.Maps.Contains(removed), "Second map was not actually registered");
                    removed.fogGrid.ClearAllFog();
                    IntVec3 common = IntVec3.Invalid;
                    for (int x = 8; x < 42 && !common.IsValid; x++)
                        for (int z = 8; z < 42 && !common.IsValid; z++)
                        {
                            var at = new IntVec3(x, 0, z);
                            if (ClearPatch(retained, at) && ClearPatch(removed, at)) common = at;
                        }
                    ctx.Require(common.IsValid, "No shared unobstructed 11x11 patch");
                    first = new WaterworksSteps.Fixture(ctx, retained, common);
                    var second = new WaterworksSteps.Fixture(ctx, removed, common);
                    var firstSluice = Prepare(ctx, first);
                    var secondSluice = Prepare(ctx, second);
                    ctx.Assert(firstSluice.parent.Position == secondSluice.parent.Position,
                        "Map-local test must use identical cell coordinates");
                    ctx.Assert(first.Canal != second.Canal && !ReferenceEquals(Grid(retained), Grid(removed)),
                        "Maps unexpectedly share Waterworks/DBH state");
                    first.Set(-1, 0, "Soil");
                    VerifySpray(ctx, firstSluice, false);
                    VerifySpray(ctx, secondSluice, true);
                    first.Set(-1, 0, "WaterMovingShallow");
                    second.Set(-1, 0, "Soil");
                    VerifySpray(ctx, firstSluice, true);
                    VerifySpray(ctx, secondSluice, false);
                    // Remove a supplied map while the same coordinates on the other map are dry.
                    first.Set(-1, 0, "Soil");
                    second.Set(-1, 0, "WaterMovingShallow");
                    VerifySpray(ctx, secondSluice, true);
                    ctx.Require((bool)Field(typeof(CanalMapComponent), "subscribed").GetValue(second.Canal),
                        "Generated map did not subscribe to native terrain events");
                    Current.Game.CurrentMap = retained;
                    Current.Game.DeinitAndRemoveMap(removed, false);
                    ctx.Assert(!Find.Maps.Contains(removed) && Find.Maps.Contains(retained), "Native map removal failed");
                    ctx.Assert(!(bool)Field(typeof(CanalMapComponent), "subscribed").GetValue(second.Canal),
                        "Removed map retained Waterworks terrain-event subscription");
                    ctx.Assert(!secondSluice.parent.Spawned, "Removed map retained a spawned source consumer");
                    ctx.Assert(!(bool)Method(typeof(MedievalCanalCompatibility), "SourceSupplies")
                        .Invoke(null, new object[] { secondSluice }), "Removed sluice retained natural supply");
                    VerifySpray(ctx, firstSluice, false);
                    first.Set(-1, 0, "WaterMovingShallow");
                    VerifySpray(ctx, firstSluice, true);
                    first.Set(-1, 0, "Soil");
                    VerifySpray(ctx, firstSluice, false);
                }
                finally
                {
                    Current.Game.CurrentMap = retained;
                    if (removed != null && Find.Maps.Contains(removed)) Current.Game.DeinitAndRemoveMap(removed, false);
                    if (parent != null && Find.WorldObjects.AllWorldObjects.Contains(parent)) Find.WorldObjects.Remove(parent);
                    if (first != null) first.Dispose();
                    Find.TickManager.CurTimeSpeed = speed;
                }
            });
        }
    }
}
