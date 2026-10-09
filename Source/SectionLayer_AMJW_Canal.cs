using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
namespace AncientMedievalJapan.Waterworks
{
    public sealed class SectionLayer_AMJW_Canal : SectionLayer
    {
        private static readonly Material[] relief = new Material[16];
        private static readonly Dictionary<TerrainDef, Material> grounds = new Dictionary<TerrainDef, Material>();
        private static Material water;
        public override bool Visible { get { return DebugViewSettings.drawTerrain; } }
        public SectionLayer_AMJW_Canal(Section section) : base(section)
        { relevantChangeTypes = MapMeshFlagDefOf.Terrain; }
        private static string Suffix(int m)
        {
            if (m == 0) return "Isolated";
            return ((m & 1) != 0 ? "N" : "") + ((m & 2) != 0 ? "E" : "") +
                ((m & 4) != 0 ? "S" : "") + ((m & 8) != 0 ? "W" : "");
        }
        private static Material Relief(int mask)
        {
            if (relief[mask] == null)
                relief[mask] = MaterialPool.MatFrom("Terrain/AMJW/Canal/AMJW_Canal_Dry_" +
                    mask.ToString("00") + "_" + Suffix(mask), ShaderDatabase.Transparent);
            return relief[mask];
        }
        private static Material Ground(TerrainDef def)
        {
            Material mat;
            if (!grounds.TryGetValue(def, out mat))
            {
                mat = new Material(def.graphic.MatSingle);
                mat.shader = ShaderDatabase.TerrainHard;
                // Canal TerrainDefs use 2388/2389. Draw the restored substrate
                // after their opaque base, before the water and relief passes.
                mat.renderQueue = 2400;
                grounds.Add(def, mat);
            }
            return mat;
        }
        public override void Regenerate()
        {
            ClearSubMeshes(MeshParts.All);
            TerrainGrid grid = Map.terrainGrid;
            CanalMapComponent canals = Map.GetComponent<CanalMapComponent>();
            if (water == null)
            {
                water = new Material(MaterialPool.MatFrom("Terrain/Surfaces/WaterShallowRamp",
                    ShaderDatabase.TerrainHard, DefDatabase<TerrainDef>.GetNamed("WaterMovingShallow").color));
                water.renderQueue = 2401;
            }
            float altitude = AltitudeLayer.TerrainScatter.AltitudeFor();
            foreach (IntVec3 cell in section.CellRect)
            {
                TerrainDef terrain = grid.TopTerrainAt(cell);
                bool wet = terrain == AMJW_Defs.AMJW_DugCanalWet;
                if (!wet && terrain != AMJW_Defs.AMJW_DugCanalDry) continue;
                if (grid.FoundationAt(cell) != null) continue;
                int mask = CanalVisualTopology.Mask(Map, cell);
                if (wet)
                    foreach (IntVec3 direction in GenAdj.CardinalDirections)
                    {
                        if (!canals.HasNaturalSourceAt(cell + direction)) continue;
                        if (direction == IntVec3.North) mask |= 1;
                        if (direction == IntVec3.East) mask |= 2;
                        if (direction == IntVec3.South) mask |= 4;
                        if (direction == IntVec3.West) mask |= 8;
                    }
                TerrainDef original = canals.OriginalTerrainAt(cell) ?? TerrainDefOf.Soil;
                Quad(GetSubMesh(Ground(original)), cell, 0, 0, 1, 1, altitude, false);
                if (wet)
                {
                    int[] runs = CanalBedGeometry.Runs[mask];
                    for (int i = 0; i < runs.Length; i += 4)
                        Quad(GetSubMesh(water), cell, runs[i]/128f, 1-runs[i+3]/128f,
                            runs[i+1]/128f, 1-runs[i+2]/128f, altitude+.002f, false);
                }
                Quad(GetSubMesh(Relief(mask)), cell, 0, 0, 1, 1, altitude+.004f, true);
            }
            FinalizeMesh(MeshParts.All);
        }
        private static void Quad(LayerSubMesh mesh, IntVec3 cell, float x0, float z0,
            float x1, float z1, float y, bool localUv)
        {
            int i = mesh.verts.Count;
            float[] xs = { x0, x0, x1, x1 }, zs = { z0, z1, z1, z0 };
            for (int n = 0; n < 4; n++)
            {
                mesh.verts.Add(new Vector3(cell.x+xs[n],y,cell.z+zs[n]));
                mesh.uvs.Add(localUv ? new Vector3(xs[n],zs[n],0) :
                    new Vector3(cell.x+xs[n],cell.z+zs[n],0));
                mesh.colors.Add(new Color32(255,255,255,255));
            }
            mesh.tris.Add(i); mesh.tris.Add(i+1); mesh.tris.Add(i+2);
            mesh.tris.Add(i); mesh.tris.Add(i+2); mesh.tris.Add(i+3);
        }
    }
}
