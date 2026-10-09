using HarmonyLib;
using RimWorld;
using Verse;

namespace AncientMedievalJapan.Waterworks
{
    /// <summary>
    /// Keep Vanilla Bridge as the actual foundation. On Waterworks canals,
    /// omit only the extra hanging bridge-props quad that Vanilla draws into
    /// the southern neighboring cell; the usual board top remains unchanged.
    /// Other bridges, foundations, pathing, support and supply are untouched.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class CanalBridgeAppearance
    {
        public const string HarmonyId = "sucro.ancientmedievaljapan.waterworks.bridgeappearance";

        static CanalBridgeAppearance()
        {
            // This is a narrowly scoped graphics hook, not a replacement
            // TerrainDef or a change to any Vanilla Bridge gameplay semantics.
            var bridgeLayer = typeof(SectionLayer).Assembly.GetType(
                "RimWorld.SectionLayer_BridgeProps", false);
            var drawCheck = bridgeLayer == null ? null : AccessTools.Method(
                bridgeLayer, "ShouldDrawPropsBelow",
                new[] { typeof(IntVec3), typeof(TerrainGrid) });
            if (drawCheck == null)
            {
                Log.Error("[AMJ Waterworks] RimWorld BridgeProps draw check unavailable; " +
                          "leaving Vanilla bridge graphics unchanged.");
                return;
            }
            new Harmony(HarmonyId).Patch(drawCheck, postfix: new HarmonyMethod(
                typeof(CanalBridgeAppearance),
                nameof(SuppressExtraPropsForCanalBridge)));
        }

        // Postfix preserves Vanilla's logic and any non-canal graphics.
        // Return false only for the native Bridge foundation on our own canals.
        public static void SuppressExtraPropsForCanalBridge(
            IntVec3 c, TerrainGrid terrGrid, ref bool __result)
        {
            if (!__result || terrGrid == null ||
                terrGrid.FoundationAt(c) != TerrainDefOf.Bridge) return;

            TerrainDef ground = terrGrid.TopTerrainAt(c);
            if (ground == AMJW_Defs.AMJW_DugCanalDry ||
                ground == AMJW_Defs.AMJW_DugCanalWet)
                __result = false;
        }
    }
}
