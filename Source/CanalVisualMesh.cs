using System;
using System.Collections.Generic;
using UnityEngine;

namespace AncientMedievalJapan.Waterworks
{
    /// <summary>
    /// Pure mesh construction, with no RimWorld draw callbacks or game-state writes.
    /// Shape is a union of at most five disjoint rectangles. The 3x3 subdivision
    /// avoids overlapping coplanar quads at elbow, T and cross junctions.
    /// World-aligned UV coordinates keep joins deterministic across cells.
    /// </summary>
    public static class CanalVisualMesh
    {
        public static Mesh Build(int mask, float halfWidth, Vector3 center, float elevation)
        {
            CanalVisualTopology.Rectangles(mask, halfWidth); // validate mask/width
            float[] x = { -0.5f, -halfWidth, halfWidth, 0.5f };
            float[] z = { -0.5f, -halfWidth, halfWidth, 0.5f };
            var vertices = new List<Vector3>(36);
            var uv = new List<Vector2>(36);
            var triangles = new List<int>(54);
            for (int ix = 0; ix < 3; ix++)
            for (int iz = 0; iz < 3; iz++)
            {
                bool inside = (ix == 1 && iz == 1) ||
                              (ix == 1 && iz == 2 && (mask & CanalVisualTopology.North) != 0) ||
                              (ix == 2 && iz == 1 && (mask & CanalVisualTopology.East) != 0) ||
                              (ix == 1 && iz == 0 && (mask & CanalVisualTopology.South) != 0) ||
                              (ix == 0 && iz == 1 && (mask & CanalVisualTopology.West) != 0);
                if (!inside) continue;
                int i = vertices.Count;
                float x0 = center.x + x[ix], x1 = center.x + x[ix + 1];
                float z0 = center.z + z[iz], z1 = center.z + z[iz + 1];
                vertices.Add(new Vector3(x0, elevation, z0));
                vertices.Add(new Vector3(x0, elevation, z1));
                vertices.Add(new Vector3(x1, elevation, z1));
                vertices.Add(new Vector3(x1, elevation, z0));
                uv.Add(new Vector2(x0,z0));
                uv.Add(new Vector2(x0,z1));
                uv.Add(new Vector2(x1,z1));
                uv.Add(new Vector2(x1,z0));
                triangles.Add(i); triangles.Add(i+1); triangles.Add(i+2);
                triangles.Add(i); triangles.Add(i+2); triangles.Add(i+3);
            }
            var mesh = new Mesh();
            mesh.name = "AMJW_Canal_" + mask;
            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
