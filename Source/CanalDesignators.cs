using RimWorld;
using UnityEngine;
using Verse;

namespace AncientMedievalJapan.Waterworks
{
    public abstract class Designator_Canal : Designator_Cells
    {
        public override int DraggableDimensions => 1;
        public override bool DragDrawMeasurements => true;
        protected abstract DesignationDef EarthworkDef { get; }
        protected abstract AcceptanceReport Validate(IntVec3 cell);

        protected Designator_Canal(string label, string desc, string iconPath)
        {
            defaultLabel = label.Translate();
            defaultDesc = desc.Translate();
            icon = ContentFinder<Texture2D>.Get(iconPath);
            useMouseIcon = true;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_Mine;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(Map) || c.Fogged(Map)) return false;
            if (Map.designationManager.DesignationAt(c, AMJW_Defs.AMJW_DigCanal) != null ||
                Map.designationManager.DesignationAt(c, AMJW_Defs.AMJW_FillCanal) != null)
                return "AMJW_AlreadyDesignated".Translate();
            return Validate(c);
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            Map.designationManager.AddDesignation(new Designation(c, EarthworkDef));
        }

        public override void SelectedUpdate() { GenUI.RenderMouseoverBracket(); }
    }

    public sealed class Designator_DigCanal : Designator_Canal
    {
        public Designator_DigCanal() :
            base("AMJW_DigLabel", "AMJW_DigDesc", "UI/Designators/Mine") { }
        protected override DesignationDef EarthworkDef => AMJW_Defs.AMJW_DigCanal;
        protected override AcceptanceReport Validate(IntVec3 c)
            => Map.GetComponent<CanalMapComponent>().CanDig(c);
    }

    public sealed class Designator_FillCanal : Designator_Canal
    {
        public Designator_FillCanal() :
            base("AMJW_FillLabel", "AMJW_FillDesc", "UI/Designators/RemoveFloor") { }
        protected override DesignationDef EarthworkDef => AMJW_Defs.AMJW_FillCanal;
        protected override AcceptanceReport Validate(IntVec3 c)
            => Map.GetComponent<CanalMapComponent>().CanFill(c);
    }
}