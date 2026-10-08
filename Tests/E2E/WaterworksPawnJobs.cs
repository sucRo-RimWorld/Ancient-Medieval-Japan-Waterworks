using System;
using System.Threading.Tasks;
using AncientMedievalJapan.Waterworks;
using RimWorks.Pickle;
using RimWorld;
using Verse;
using Verse.AI;

namespace AncientMedievalJapan.Waterworks.E2E
{
    [PickleSteps]
    public sealed class WaterworksPawnJobs
    {
        [Then("a construction pawn actually digs and fills a canal")]
        public async Task Work(PickleContext ctx)
        {
            WaterworksSteps.Fixture fixture = null;
            Pawn worker = null;
            TimeSpeed oldSpeed = TimeSpeed.Normal;
            try
            {
                await GameThread.Run(delegate
                {
                    oldSpeed = Find.TickManager.CurTimeSpeed;
                    Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                    fixture = new WaterworksSteps.Fixture(ctx);
                    for (int i = 0; i < 30; i++)
                    {
                        Pawn candidate = PawnGenerator.GeneratePawn(
                            PawnKindDefOf.Colonist, Faction.OfPlayer);
                        if (!candidate.Downed && candidate.skills != null &&
                            !candidate.WorkTypeIsDisabled(WorkTypeDefOf.Construction))
                        {
                            worker = candidate;
                            break;
                        }
                        candidate.Destroy(DestroyMode.Vanish);
                    }
                    ctx.Require(worker != null, "No capable Construction worker.");
                    GenSpawn.Spawn(worker, fixture.Cell(0, 1), fixture.Map);
                    worker.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
                    worker.skills.GetSkill(SkillDefOf.Construction).Level = 15;
                    fixture.Set(-1, 0, "WaterMovingShallow");
                });

                IntVec3 target = IntVec3.Invalid;
                Job dig = null;
                await GameThread.Run(delegate
                {
                    target = fixture.Cell(0, 0);
                    fixture.Map.designationManager.AddDesignation(
                        new Designation(target, AMJW_Defs.AMJW_DigCanal));
                    WorkGiver_Scanner giver = (WorkGiver_Scanner)
                        DefDatabase<WorkGiverDef>.GetNamed("AMJW_DigCanalWork").Worker;
                    ctx.Require(giver.HasJobOnCell(worker, target, true),
                        "Dig WorkGiver refused eligible order.");
                    dig = giver.JobOnCell(worker, target, true);
                    ctx.Require(dig != null && dig.def == AMJW_Defs.AMJW_DigCanalJob,
                        "Dig WorkGiver offered wrong JobDef.");
                    Start(worker, dig, ctx, "dig");
                });
                await Complete(worker, dig, () => fixture.Canal.IsCanal(target), ctx, "dig");
                await GameThread.Run(delegate
                {
                    fixture.Wet(target, "pawn excavation should yield supplied canal");
                    ctx.Assert(fixture.Map.designationManager.DesignationAt(
                        target, AMJW_Defs.AMJW_DigCanal) == null,
                        "Dig designation not consumed.");
                });

                Job fill = null;
                await GameThread.Run(delegate
                {
                    fixture.Map.designationManager.AddDesignation(
                        new Designation(target, AMJW_Defs.AMJW_FillCanal));
                    WorkGiver_Scanner giver = (WorkGiver_Scanner)
                        DefDatabase<WorkGiverDef>.GetNamed("AMJW_FillCanalWork").Worker;
                    ctx.Require(giver.HasJobOnCell(worker, target, true),
                        "Fill WorkGiver refused eligible order.");
                    fill = giver.JobOnCell(worker, target, true);
                    ctx.Require(fill != null && fill.def == AMJW_Defs.AMJW_FillCanalJob,
                        "Fill WorkGiver offered wrong JobDef.");
                    Start(worker, fill, ctx, "fill");
                });
                await Complete(worker, fill, () => !fixture.Canal.IsCanal(target), ctx, "fill");
                await GameThread.Run(delegate
                {
                    ctx.Assert(fixture.Map.terrainGrid.TopTerrainAt(target) ==
                        TerrainDefOf.Soil, "Pawn did not restore original Soil.");
                    ctx.Assert(fixture.Map.designationManager.DesignationAt(
                        target, AMJW_Defs.AMJW_FillCanal) == null,
                        "Fill designation not consumed.");
                });
            }
            finally
            {
                await GameThread.Run(delegate
                {
                    if (worker != null)
                    {
                        if (worker.CurJob != null)
                            worker.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
                        worker.Destroy(DestroyMode.Vanish);
                    }
                    if (fixture != null) fixture.Dispose();
                    Find.TickManager.CurTimeSpeed = oldSpeed;
                });
            }
        }

        private static void Start(Pawn worker, Job job, PickleContext ctx, string label)
        {
            if (worker.needs.food != null) worker.needs.food.CurLevelPercentage = 1f;
            if (worker.needs.rest != null) worker.needs.rest.CurLevelPercentage = 1f;
            job.playerForced = true;
            worker.jobs.StartJob(job, JobCondition.InterruptForced);
            ctx.Require(worker.CurJob == job, label + " failed to start.");
        }

        private static async Task Complete(Pawn worker, Job job, Func<bool> success,
            PickleContext ctx, string label)
        {
            bool completed = false;
            for (int batch = 0; batch < 100 && !completed; batch++)
            {
                await GameThread.Run(delegate
                {
                    for (int tick = 0; tick < 64; tick++)
                    {
                        if (success()) { completed = true; break; }
                        ctx.Require(worker.CurJob == job,
                            label + " ended without changing terrain.");
                        Find.TickManager.DoSingleTick();
                    }
                    if (success()) completed = true;
                });
                if (!completed) await Task.Delay(1);
            }
            await GameThread.Run(delegate
            {
                ctx.Require(completed, label + " exceeded 6400 game ticks.");
            });
        }
    }
}
