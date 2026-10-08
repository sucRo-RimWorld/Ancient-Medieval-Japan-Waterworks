using System;
using Verse;

namespace AncientMedievalJapan.Waterworks
{
    /// <summary>
    /// Rendering-only geometry description for one canal tile.
    /// No changes to water supply, terrain identity, paths or saved data.
    /// Cardinal masks give 16 stable end/line/elbow/T/cross variants.
    /// </summary>
    public static class CanalVisualTopology
    {
        public const int North = 1;
        public const int East = 2;
        public const int South = 4;
        public const int West = 8;
        public const float HalfChannel = 0.20f;
        public const float HalfBank = 0.34f;

        public struct Rectangle
        {
            public readonly float XMin, ZMin, XMax, ZMax;
            public Rectangle(float x0, float z0, float x1, float z1)
            {
                XMin = x0; ZMin = z0; XMax = x1; ZMax = z1;
            }
        }

        public static int Mask(Map map, IntVec3 cell)
        {
            int mask = 0;
            if (CanalAt(map, cell + IntVec3.North)) mask |= North;
            if (CanalAt(map, cell + IntVec3.East)) mask |= East;
            if (CanalAt(map, cell + IntVec3.South)) mask |= South;
            if (CanalAt(map, cell + IntVec3.West)) mask |= West;
            return mask;
        }

        public static bool CanalAt(Map map, IntVec3 cell)
        {
            if (map == null || !cell.InBounds(map)) return false;
            TerrainDef terrain = map.terrainGrid.TopTerrainAt(cell);
            return terrain == AMJW_Defs.AMJW_DugCanalWet ||
                   terrain == AMJW_Defs.AMJW_DugCanalDry;
        }

        /// <summary>
        /// Generates an entirely local cross-section from the cardinal mask.
        /// The core always remains inside the cell; arms reach its matching edge.
        /// This works for narrow water surfaces and a slightly wider soil-bank rim.
        /// A renderer should draw a connected rectangle union rather than
        /// overlapping individual quads with visible seams.
        /// </summary>
        public static Rectangle[] Rectangles(int mask, float halfWidth)
        {
            if ((mask & ~15) != 0)
                throw new ArgumentOutOfRangeException("mask");
            if (!(halfWidth > 0f && halfWidth < 0.5f))
                throw new ArgumentOutOfRangeException("halfWidth");
            Rectangle[] result = new Rectangle[1 + Count(mask)];
            int index = 0;
            result[index++] = new Rectangle(-halfWidth, -halfWidth, halfWidth, halfWidth);
            if ((mask & North) != 0)
                result[index++] = new Rectangle(-halfWidth, halfWidth, halfWidth, 0.5f);
            if ((mask & East) != 0)
                result[index++] = new Rectangle(halfWidth, -halfWidth, 0.5f, halfWidth);
            if ((mask & South) != 0)
                result[index++] = new Rectangle(-halfWidth, -0.5f, halfWidth, -halfWidth);
            if ((mask & West) != 0)
                result[index++] = new Rectangle(-0.5f, -halfWidth, -halfWidth, halfWidth);
            return result;
        }

        private static int Count(int mask)
        {
            int n = 0;
            while (mask != 0) { n += mask & 1; mask >>= 1; }
            return n;
        }
    }
}
