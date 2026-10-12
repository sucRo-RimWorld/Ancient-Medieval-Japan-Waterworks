using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace AncientMedievalJapan.Waterworks
{
    /// <summary>
    /// Optional adapter: DBH for Medieval sluices normally require water in a
    /// DBH storage tower. A supplied Waterworks open canal is a separate natural
    /// source: it may irrigate without inventing a DBH tank or mutating PipeNet.
    /// Reflection is required because DBHforMedieval types are internal and DBH
    /// is not a hard dependency.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class MedievalCanalCompatibility
    {
        private static readonly Type SluiceType =
            AccessTools.TypeByName("DBHforMedieval.CompSluiceGate");
        private static readonly Type CanalType =
            AccessTools.TypeByName("DBHforMedieval.Building_IrrigationCanal");
        private static readonly Type UtilityType =
            AccessTools.TypeByName("DBHforMedieval.IrrigationUtility");
        private static readonly MethodInfo ActiveCellsMethod =
            SluiceType == null ? null : AccessTools.Method(SluiceType, "ActiveCells");
        private static readonly MethodInfo DriedCellsMethod =
            UtilityType == null ? null : AccessTools.Method(UtilityType, "IsDriedCells");
        private static readonly MethodInfo AddMoistureMethod =
            UtilityType == null ? null : AccessTools.Method(
                UtilityType, "AddMoisture",
                new[] { typeof(Map), typeof(IntVec3), typeof(float), typeof(float) });
        private static readonly FieldInfo PipeField =
            SluiceType == null ? null : AccessTools.Field(SluiceType.BaseType, "PipeComp");
        private static readonly FieldInfo PropsField =
            SluiceType == null ? null : AccessTools.Field(SluiceType.BaseType, "props");
        private static readonly FieldInfo FireDamageField =
            AccessTools.TypeByName("DubsBadHygiene.CompProperties_Sprinkler") == null
                ? null : AccessTools.Field(
                    AccessTools.TypeByName("DubsBadHygiene.CompProperties_Sprinkler"), "fireDamage");
        private static readonly MethodInfo CoolingMethod =
            SluiceType == null ? null : AccessTools.Method(SluiceType.BaseType, "Cooling");

        static MedievalCanalCompatibility()
        {
            if (SluiceType == null || CanalType == null || UtilityType == null)
                return;
            MethodInfo wait = AccessTools.Method(SluiceType, "IsWaitingForDoer");
            MethodInfo spray = AccessTools.Method(SluiceType, "TrySpray");
            MethodInfo flow = AccessTools.PropertyGetter(CanalType, "WaterFlow");
            if (wait == null || spray == null || flow == null ||
                ActiveCellsMethod == null || DriedCellsMethod == null ||
                AddMoistureMethod == null || PipeField == null ||
                CoolingMethod == null || FireDamageField == null)
            {
                Log.Warning("[AMJ Waterworks] DBH for Medieval irrigation API mismatch; adapter disabled.");
                return;
            }
            var harmony = new Harmony("sucro.amjw.medievalcanal");
            harmony.Patch(wait, postfix: new HarmonyMethod(
                typeof(MedievalCanalCompatibility), nameof(WaitingPostfix)));
            harmony.Patch(spray, prefix: new HarmonyMethod(
                typeof(MedievalCanalCompatibility), nameof(SprayPrefix)));
            harmony.Patch(flow, postfix: new HarmonyMethod(
                typeof(MedievalCanalCompatibility), nameof(FlowPostfix)));
        }

        private static bool SourceSupplies(object sluice)
        {
            var comp = sluice as ThingComp;
            if (comp == null || comp.parent == null || !comp.parent.Spawned) return false;
            var parent = comp.parent;
            var pipe = PipeField.GetValue(sluice) as ThingComp;
            if (pipe == null || pipe.parent == null ||
                pipe.parent.def == null ||
                pipe.parent.def.defName != "ES_IrrigationCanal") return false;
            var map = parent.Map;
            if (map == null || pipe.parent.Map != map) return false;
            var waterworks = map.GetComponent<CanalMapComponent>();
            return waterworks != null &&
                waterworks.MedievalIrrigationCanalTouchesWaterworksSource(pipe.parent.Position);
        }

        private static List<IntVec3> Cells(object sluice)
        {
            var cells = ActiveCellsMethod.Invoke(sluice, null) as IEnumerable<IntVec3>;
            return cells == null ? new List<IntVec3>() : cells.ToList();
        }

        // Preserve an already valid DBH irrigation request. Only extend failure
        // when a naturally supplied Waterworks canal feeds this Medieval chain.
        public static void WaitingPostfix(object __instance, ref bool __result)
        {
            if (__result || !SourceSupplies(__instance)) return;
            var comp = (ThingComp)__instance;
            var cells = Cells(__instance);
            if (cells.Count == 0) return;
            __result = (bool)DriedCellsMethod.Invoke(null,
                new object[] { comp.parent.Map, cells });
        }

        // Use original DBH implementation when its own tower has enough water.
        // Otherwise the supplied open canal irrigates independently without
        // fabricating water towers or draining unrelated DBH consumers.
        public static bool SprayPrefix(object __instance, ref bool __result)
        {
            if (!SourceSupplies(__instance)) return true;
            var comp = (ThingComp)__instance;
            var cells = Cells(__instance);
            if (cells.Count == 0) { __result = false; return false; }
            // The native sluice's only water gate checks existing DBH towers.
            // A naturally fed canal takes precedence so DBH storage is untouched.
            float fireDamage = Convert.ToSingle(FireDamageField.GetValue(comp.props));
            CoolingMethod.Invoke(__instance, null);
            var args = new object[4];
            args[0] = comp.parent.Map;
            args[2] = 5f;
            args[3] = fireDamage;
            foreach (var cell in cells)
            {
                args[1] = cell;
                AddMoistureMethod.Invoke(null, args);
            }
            __result = true;
            return false;
        }

        public static void FlowPostfix(object __instance, ref bool __result)
        {
            if (__result) return;
            var building = __instance as Thing;
            if (building == null || !building.Spawned) return;
            var waterworks = building.Map.GetComponent<CanalMapComponent>();
            __result = waterworks != null &&
                waterworks.MedievalIrrigationCanalTouchesWaterworksSource(building.Position);
        }
    }
}
