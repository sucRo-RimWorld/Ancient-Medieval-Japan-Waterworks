using RimWorld;
using Verse;
using Verse.AI;

namespace AncientMedievalJapan.Waterworks
{
    public abstract class WorkGiver_Canal : WorkGiver_ConstructAffectFloor
    {
        protected abstract JobDef EarthworkJob { get; }
        protected abstract AcceptanceReport CanAffect(CanalMapComponent component, IntVec3 cell);

        public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
        {
            return base.HasJobOnCell(pawn, c, forced) &&
                CanAffect(pawn.Map.GetComponent<CanalMapComponent>(), c).Accepted;
        }

        public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
        {
            if (!HasJobOnCell(pawn, c, forced)) return null;
            return JobMaker.MakeJob(EarthworkJob, c);
        }
    }

    public sealed class WorkGiver_DigCanal : WorkGiver_Canal
    {
        protected override DesignationDef DesDef => AMJW_Defs.AMJW_DigCanal;
        protected override JobDef EarthworkJob => AMJW_Defs.AMJW_DigCanalJob;
        protected override AcceptanceReport CanAffect(CanalMapComponent component, IntVec3 cell)
            => component.CanDig(cell, allowWildPlants: true);

        public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
        {
            if (!HasJobOnCell(pawn, c, forced)) return null;
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
        protected override AcceptanceReport CanAffect(CanalMapComponent component, IntVec3 cell)
            => component.CanFill(cell);
    }

    public abstract class JobDriver_Canal : JobDriver_AffectFloor
    {
        protected override StatDef SpeedStat => StatDefOf.ConstructionSpeed;
        protected abstract AcceptanceReport CanAffect(CanalMapComponent component, IntVec3 cell);

        protected JobDriver_Canal() { clearSnow = true; }

        protected override System.Collections.Generic.IEnumerable<Toil> MakeNewToils()
        {
            // JobDriver_AffectFloor deletes its designation even when DoEffect
            // cannot alter terrain. Abort first if the cell changes during work.
            this.FailOn(() => !CanAffect(Map.GetComponent<CanalMapComponent>(), TargetLocA).Accepted);
            foreach (Toil toil in base.MakeNewToils())
                yield return toil;
        }
    }

    public sealed class JobDriver_DigCanal : JobDriver_Canal
    {
        protected override int BaseWorkAmount => 500;
        protected override DesignationDef DesDef => AMJW_Defs.AMJW_DigCanal;
        protected override AcceptanceReport CanAffect(CanalMapComponent component, IntVec3 cell)
            => component.CanDig(cell);
        protected override void DoEffect(IntVec3 c)
            => Map.GetComponent<CanalMapComponent>().Dig(c);
    }

    public sealed class JobDriver_FillCanal : JobDriver_Canal
    {
        protected override int BaseWorkAmount => 300;
        protected override DesignationDef DesDef => AMJW_Defs.AMJW_FillCanal;
        protected override AcceptanceReport CanAffect(CanalMapComponent component, IntVec3 cell)
            => component.CanFill(cell);
        protected override void DoEffect(IntVec3 c)
            => Map.GetComponent<CanalMapComponent>().Fill(c);
    }
}