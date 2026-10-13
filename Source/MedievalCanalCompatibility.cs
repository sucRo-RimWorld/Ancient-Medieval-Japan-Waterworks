using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
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
        private static readonly FieldInfo FireDamageField =
            AccessTools.TypeByName("DubsBadHygiene.CompProperties_Sprinkler") == null
                ? null : AccessTools.Field(
                    AccessTools.TypeByName("DubsBadHygiene.CompProperties_Sprinkler"), "fireDamage");
        private static readonly FieldInfo RippleDefField =
            AccessTools.TypeByName("DBHforMedieval.DBHfM_DefOf") == null ? null :
                AccessTools.Field(AccessTools.TypeByName("DBHforMedieval.DBHfM_DefOf"), "ES_Mote_Ripple");
        private static readonly MethodInfo CoolingMethod =
            SluiceType == null ? null : AccessTools.Method(SluiceType.BaseType, "Cooling");

        // DBH for Medieval 1.6 APIs are not guaranteed on the 1.7 unstable
        // branch: patch only the exact decompiled signatures we have audited.
        private static bool ReturnsBoolWithoutArguments(MethodInfo method)
        {
            return method != null && method.ReturnType == typeof(bool) &&
                method.GetParameters().Length == 0;
        }

        private static bool MatchesIrrigationMethods()
        {
            if (DriedCellsMethod == null ||
                DriedCellsMethod.ReturnType != typeof(bool)) return false;
            var driedArgs = DriedCellsMethod.GetParameters();
            return driedArgs.Length == 2 &&
                driedArgs[0].ParameterType == typeof(Map) &&
                driedArgs[1].ParameterType == typeof(List<IntVec3>);
        }

        static MedievalCanalCompatibility()
        {
            if (SluiceType == null || CanalType == null || UtilityType == null)
                return;
            MethodInfo wait = AccessTools.Method(SluiceType, "IsWaitingForDoer");
            MethodInfo spray = AccessTools.Method(SluiceType, "TrySpray");
            MethodInfo flow = AccessTools.PropertyGetter(CanalType, "WaterFlow");
            if (!ReturnsBoolWithoutArguments(wait) ||
                !ReturnsBoolWithoutArguments(spray) ||
                !ReturnsBoolWithoutArguments(flow) ||
                ActiveCellsMethod == null || !MatchesIrrigationMethods() ||
                AddMoistureMethod == null ||
                AddMoistureMethod.ReturnType != typeof(void) ||
                PipeField == null ||
                AccessTools.Field(SluiceType, "width") == null ||
                AccessTools.Field(SluiceType, "length") == null ||
                CoolingMethod == null ||
                CoolingMethod.ReturnType != typeof(void) ||
                CoolingMethod.GetParameters().Length != 0 ||
                FireDamageField == null)
            {
                Log.Warning("[AMJ Waterworks] DBH for Medieval irrigation API mismatch; adapter disabled.");
                return;
            }
            var harmony = new Harmony("sucro.amjw.medievalcanal");
            harmony.Patch(wait, postfix: new HarmonyMethod(
                typeof(MedievalCanalCompatibility), nameof(WaitingPostfix)));
            harmony.Patch(spray, prefix: new HarmonyMethod(
                typeof(MedievalCanalCompatibility), nameof(SprayPrefix)));
            harmony.Patch(flow, prefix: new HarmonyMethod(
                typeof(MedievalCanalCompatibility), nameof(FlowPrefix)),
                postfix: new HarmonyMethod(
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

        // Native tower-supplied irrigation takes precedence over the optional
        // natural canal. Never bypass the ordinary DBH water charge when storage
        // can already satisfy this sluice's own full-area usage requirement.
        private static bool CanUseNativeStorage(object sluice)
        {
            var pipe = PipeField.GetValue(sluice);
            if (pipe == null) return false;
            var netProperty = AccessTools.Property(pipe.GetType(), "pipeNet");
            if (netProperty == null) return false;
            var net = netProperty.GetValue(pipe, null);
            if (net == null) return false;
            var towersField = AccessTools.Field(net.GetType(), "WaterTowers");
            var towersProperty = AccessTools.Property(net.GetType(), "WaterTowers");
            var towers = (towersField != null ? towersField.GetValue(net) :
                towersProperty == null ? null : towersProperty.GetValue(net, null))
                as System.Collections.IEnumerable;
            if (towers == null) return false;
            var comp = sluice as ThingComp;
            if (comp == null || comp.parent == null) return false;
            float usage = 0f;
            var props = comp.props;
            var usageField = props == null ? null :
                AccessTools.Field(props.GetType(), "usagePerCell");
            if (usageField == null) return false;
            float perCell = Convert.ToSingle(usageField.GetValue(props));
            var lengthField = AccessTools.Field(SluiceType, "length");
            var widthField = AccessTools.Field(SluiceType, "width");
            if (lengthField == null || widthField == null) return false;
            usage = Convert.ToSingle(lengthField.GetValue(sluice)) *
                Convert.ToSingle(widthField.GetValue(sluice)) * perCell;
            if (usage <= 0f) return false;
            float available = 0f;
            foreach (object tower in towers)
            {
                if (tower == null) continue;
                var storage = AccessTools.Field(tower.GetType(), "WaterStorage");
                if (storage != null)
                    available += Convert.ToSingle(storage.GetValue(tower));
            }
            return available >= usage;
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

        // Preserve ordinary DBH execution when no natural canal is connected.
        // With natural supply, irrigate independently without fabricating
        // water towers or draining unrelated DBH consumers.
        public static bool SprayPrefix(object __instance, ref bool __result)
        {
            if (!SourceSupplies(__instance) || CanUseNativeStorage(__instance)) return true;
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
            // Preserve the original Medieval sluice ripple for its manual gate.
            if (RippleDefField != null)
            {
                var rippleDef = RippleDefField.GetValue(null) as FleckDef;
                if (rippleDef != null)
                {
                    var position = comp.parent.Position.ToVector3Shifted() +
                        comp.parent.Rotation.FacingCell.ToVector3() * 0.5f;
                    FleckCreationData ripple = FleckMaker.GetDataStatic(
                        position, comp.parent.Map, rippleDef, 1f);
                    ripple.rotation = comp.parent.Rotation.AsAngle;
                    comp.parent.Map.flecks.CreateFleck(ripple);
                }
            }
            __result = true;
            return false;
        }

        // The upstream WaterFlow getter directly dereferences pipe.pipeNet.
        // An isolated Medieval canal may temporarily have no DBH network.
        // Skip only that unsafe native path; the postfix still checks the
        // naturally supplied Waterworks canal chain.
        public static bool FlowPrefix(object __instance, ref bool __result)
        {
            var building = __instance as ThingWithComps;
            if (building == null || !building.Spawned) return true;
            var pipe = building.AllComps.FirstOrDefault(comp =>
                comp != null && comp.GetType().FullName == "DubsBadHygiene.CompPipe");
            if (pipe == null) { __result = false; return false; }
            var networkProperty = AccessTools.Property(pipe.GetType(), "pipeNet");
            if (networkProperty == null || networkProperty.GetValue(pipe, null) == null)
            {
                __result = false;
                return false;
            }
            return true;
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
