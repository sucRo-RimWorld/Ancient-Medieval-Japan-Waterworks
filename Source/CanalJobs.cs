using RimWorld;
using Verse;
using Verse.AI;

namespace AncientMedievalJapan.Waterworks
{
    public abstract class WorkGiver_Canal : WorkGiver_ConstructAffectFloor
    {
        protected abstract JobDef EarthworkJob { get; }
        public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
            => JobMaker.MakeJob(EarthworkJob, c);
    }

    public sealed class WorkGiver_DigCanal : WorkGiver_Canal
    {
        protected override DesignationDef DesDef => AMJW_Defs.AMJW_DigCanal;
        protected override JobDef EarthworkJob => AMJW_Defs.AMJW_DigCanalJob;

        public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
        {
            Plant wildPlant = c.GetPlant(pawn.Map);
            if (wildPlant != null)
            {
                if (wildPlant.sown || wildPlant.IsForbidden(pawn) ||
                    !pawn.CanReserve(wildPlant, 1, -1, null, forced) ||
                    !PlantUtility.PawnWillingToCutPlant_Job(wildPlant, pawn))
                    return null;
                // Do not excavate until the plant is removed. RimWorld owns
                // the plant-cutting job and its effects.
                return JobMaker.MakeJob(JobDefOf.CutPlant, wildPlant);
            }
            return base.JobOnCell(pawn, c, forced);
        }
    }

    public sealed class WorkGiver_FillCanal : WorkGiver_Canal
    {
        protected override DesignationDef DesDef => AMJW_Defs.AMJW_FillCanal;
        protected override JobDef EarthworkJob => AMJW_Defs.AMJW_FillCanalJob;
    }

    public abstract class JobDriver_Canal : JobDriver_AffectFloor
    {
        protected override StatDef SpeedStat => StatDefOf.ConstructionSpeed;
        protected JobDriver_Canal() { clearSnow = true; }
    }

    public sealed class JobDriver_DigCanal : JobDriver_Canal
    {
        protected override int BaseWorkAmount => 500;
        protected override DesignationDef DesDef => AMJW_Defs.AMJW_DigCanal;
        protected override void DoEffect(IntVec3 c)
            => Map.GetComponent<CanalMapComponent>().Dig(c);
    }

    public sealed class JobDriver_FillCanal : JobDriver_Canal
    {
        protected override int BaseWorkAmount => 300;
        protected override DesignationDef DesDef => AMJW_Defs.AMJW_FillCanal;
        protected override void DoEffect(IntVec3 c)
            => Map.GetComponent<CanalMapComponent>().Fill(c);
    }
}