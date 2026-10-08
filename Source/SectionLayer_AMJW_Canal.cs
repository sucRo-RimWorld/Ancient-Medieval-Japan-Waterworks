using RimWorld;
using UnityEngine;
using Verse;

namespace AncientMedievalJapan.Waterworks
{
    /// <summary>
    /// 1.6 section-mesh prototype. Section discovers concrete SectionLayer
    /// subclasses automatically. The gameplay terrain stays authoritative;
    /// only the narrow bank and wet/dry channel are drawn here.
    /// </summary>
    public sealed class SectionLayer_AMJW_Canal : SectionLayer
    {
        private static Material bankMaterial;
        private static Material wetMaterial;
        private static Material dryMaterial;

        public override bool Visible { get { return DebugViewSettings.drawTerrain; } }

        public SectionLayer_AMJW_Canal(Section section) : base(section)
        {
            relevantChangeTypes = MapMeshFlagDefOf.Terrain;
        }

        public override void Regenerate()
        {
            ClearSubMeshes(MeshParts.All);
            TerrainGrid grid = Map.terrainGrid;
            // MaterialPool is main-thread only. Resolve lazily while regenerating.
            if (bankMaterial == null)
                bankMaterial = MaterialPool.MatFrom("Terrain/Surfaces/Soil",
                    ShaderDatabase.TerrainHard, new Color(0.78f, 0.69f, 0.57f));
            if (wetMaterial == null)
                wetMaterial = MaterialPool.MatFrom("Terrain/Surfaces/WaterShallowRamp",
                    ShaderDatabase.TerrainHard, new Color(0.81f, 0.91f, 1f));
            if (dryMaterial == null)
                dryMaterial = MaterialPool.MatFrom("Terrain/Surfaces/Soil",
                    ShaderDatabase.TerrainHard, new Color(0.53f, 0.44f, 0.35f));
            float bankAltitude = AltitudeLayer.TerrainScatter.AltitudeFor();
            float channelAltitude = bankAltitude + 0.001f;

            foreach (IntVec3 cell in section.CellRect)
            {
                TerrainDef terrain = grid.TopTerrainAt(cell);
                bool wet = terrain == AMJW_Defs.AMJW_DugCanalWet;
                if (!wet && terrain != AMJW_Defs.AMJW_DugCanalDry)
                    continue;
                // The Vanilla bridge/foundation remains responsible for covering
                // a canal. Never draw a canal over its foundation or props.
                if (grid.FoundationAt(cell) != null)
                    continue;

                int mask = CanalVisualTopology.Mask(Map, cell);
                if (wet)
                    mask = ConnectNaturalWaterAtMouth(mask, cell);
                Vector3 middle = new Vector3(cell.x + 0.5f, 0f, cell.z + 0.5f);
                CanalVisualMesh.Append(GetSubMesh(bankMaterial), mask,
                    CanalVisualTopology.HalfBank, middle, bankAltitude);
                CanalVisualMesh.Append(GetSubMesh(wet ? wetMaterial : dryMaterial), mask,
                    CanalVisualTopology.HalfChannel, middle, channelAltitude);
            }
            FinalizeMesh(MeshParts.All);
        }

        private int ConnectNaturalWaterAtMouth(int mask, IntVec3 cell)
        {
            if (IsMovingFreshWater(cell + IntVec3.North)) mask |= CanalVisualTopology.North;
            if (IsMovingFreshWater(cell + IntVec3.East)) mask |= CanalVisualTopology.East;
            if (IsMovingFreshWater(cell + IntVec3.South)) mask |= CanalVisualTopology.South;
            if (IsMovingFreshWater(cell + IntVec3.West)) mask |= CanalVisualTopology.West;
            return mask;
        }

        private bool IsMovingFreshWater(IntVec3 cell)
        {
            if (!cell.InBounds(Map))
                return false;
            string name = Map.terrainGrid.TopTerrainAt(cell).defName;
            return name == "WaterMovingShallow" || name == "WaterMovingChestDeep";
        }
    }
}
