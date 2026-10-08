using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AncientMedievalJapan.Waterworks;
using RimWorks.Pickle;
using RimWorld;
using UnityEngine;
using Verse;

namespace AncientMedievalJapan.Waterworks.E2E
{
    [PickleSteps]
    public sealed class WaterworksVisualSteps
    {
        private static readonly List<IntVec3> Channels = new List<IntVec3>();
        private static Map map;
        private static CanalMapComponent canal;
        private static IntVec3 center;
        private static string output;
        private static IntVec3 C(int x, int z) { return center + new IntVec3(x, 0, z); }

        [Then("Waterworks renders the connected disconnected and restored canal scene")]
        public async Task Capture(PickleContext context)
        {
            await GameThread.Run(delegate {
                map = Find.CurrentMap;
                context.Require(map != null && map.Size.x >= 40 && map.Size.z >= 40, "Quickstart map missing");
                canal = map.GetComponent<CanalMapComponent>();
                context.Require(canal != null, "Canal map component missing");
                IntVec3 selected = IntVec3.Invalid;
                int bestDistance = int.MaxValue;
                for (int x = 9; x < map.Size.x - 9; x++)
                    for (int z = 9; z < map.Size.z - 9; z++) {
                        IntVec3 origin = new IntVec3(x, 0, z);
                        bool safe = true;
                        for (int dx = -8; dx <= 8 && safe; dx++)
                            for (int dz = -8; dz <= 8; dz++) {
                                IntVec3 p = origin + new IntVec3(dx, 0, dz);
                                if (p.Fogged(map) || p.GetFirstBuilding(map) != null ||
                                    map.terrainGrid.FoundationAt(p) != null ||
                                    map.terrainGrid.UnderTerrainAt(p) != null) { safe = false; break; }
                            }
                        if (safe) {
                            int distance = Math.Abs(x - map.Size.x / 2) + Math.Abs(z - map.Size.z / 2);
                            if (distance < bestDistance) { selected = origin; bestDistance = distance; }
                        }
                    }
                context.Require(selected.IsValid, "No revealed, foundation-free 17x17 scene patch");
                center = selected;
                for (int x = -8; x <= 8; x++)
                    for (int z = -8; z <= 8; z++) {
                        IntVec3 p = C(x,z);
                        foreach (Thing t in new List<Thing>(p.GetThingList(map)))
                            if (t is Plant) t.Destroy(DestroyMode.Vanish);
                        map.terrainGrid.SetTerrain(p, TerrainDefOf.Soil);
                    }
                for (int x = 2; x <= 5; x++)
                    map.terrainGrid.SetTerrain(C(x,-6), DefDatabase<TerrainDef>.GetNamed("Gravel"));
                Channels.Clear();
                for (int x = -5; x <= 5; x++) Channels.Add(C(x,0));
                for (int z = -4; z <= 5; z++) if (z != 0) Channels.Add(C(0,z));
                for (int z = 1; z <= 3; z++) Channels.Add(C(4,z));
                for (int x = 2; x <= 3; x++) Channels.Add(C(x,3));
                for (int x = -5; x <= -2; x++) Channels.Add(C(x,-5));
                foreach (IntVec3 p in Channels)
                    context.Require(canal.Dig(p), "Dig failed at " + p);
                Source(true);
                map.terrainGrid.SetFoundation(C(-2,0), TerrainDefOf.Bridge);
                map.terrainGrid.SetFoundation(C(0,2), TerrainDefOf.Bridge);
                AssertState(context, true);
                context.Assert(CanalVisualTopology.Mask(map, C(0,0)) == 15, "Cross junction mask");
                context.Assert(CanalVisualTopology.Mask(map, C(4,3)) == 12, "North return elbow mask");
                context.Assert(CanalVisualTopology.Mask(map, C(-3,0)) == 10, "Horizontal trunk mask");
                context.Assert(CanalVisualTopology.Rectangles(15, CanalVisualTopology.HalfChannel).Length == 5,
                    "Cross geometry expected core plus four arms");
                Find.CameraDriver.JumpToCurrentMapLoc(center);
                output = Path.Combine(GenFilePaths.SaveDataFolderPath, "WaterworksVisual");
                Directory.CreateDirectory(output);
                });
            await Task.Delay(1500); // Let terrain meshes and the camera render the connected state.
            await GameThread.Run(delegate {
                ScreenCapture.CaptureScreenshot(Path.Combine(output,"connected.png"));
            });
            await WaitFile("connected.png", context);
            await GameThread.Run(delegate {
                Source(false);
                AssertState(context, false);
            });
            await Task.Delay(1500);
            await GameThread.Run(delegate {
                ScreenCapture.CaptureScreenshot(Path.Combine(output,"disconnected.png"));
            });
            await WaitFile("disconnected.png", context);
            await GameThread.Run(delegate {
                Source(true);
                AssertState(context, true);
            });
            await Task.Delay(1500);
            await GameThread.Run(delegate {
                ScreenCapture.CaptureScreenshot(Path.Combine(output,"restored.png"));
            });
            await WaitFile("restored.png", context);
            await GameThread.Run(delegate {
                File.WriteAllText(Path.Combine(output, "manifest.txt"),
                    "seed=AMJ-Waterworks-E2E\nmapSize=50\n" +
                    "images=connected.png,disconnected.png,restored.png\n" +
                    "resolution=" + Screen.width + "x" + Screen.height + "\n" +
                    "cameraCenter=" + center + "\n");
            });
        }
        private static void Source(bool connected)
        {
            TerrainDef t = DefDatabase<TerrainDef>.GetNamed(connected ? "WaterMovingShallow" : "Soil");
            map.terrainGrid.SetTerrain(C(-7,0), t);
            map.terrainGrid.SetTerrain(C(-6,0), t);
            canal.Recalculate();
        }
        private static void AssertState(PickleContext c, bool supplied)
        {
            string name = supplied ? "AMJW_DugCanalWet" : "AMJW_DugCanalDry";
            c.Assert(map.terrainGrid.TopTerrainAt(C(0,0)).defName == name, "Trunk supply state");
            c.Assert(map.terrainGrid.TopTerrainAt(C(0,5)).defName == name, "Branch supply state");
            c.Assert(map.terrainGrid.TopTerrainAt(C(-4,-5)).defName == "AMJW_DugCanalDry", "Isolated dry section");
            c.Assert(map.terrainGrid.FoundationAt(C(-2,0)) == TerrainDefOf.Bridge, "Trunk bridge");
            c.Assert(map.terrainGrid.FoundationAt(C(0,2)) == TerrainDefOf.Bridge, "Branch bridge");
        }
        private static async Task WaitFile(string file, PickleContext c)
        {
            string path = Path.Combine(output, file);
            for (int i = 0; i < 60; i++) {
                await Task.Delay(500);
                if (File.Exists(path) && new FileInfo(path).Length > 24) {
                    using (var stream = File.OpenRead(path))
                    using (var reader = new BinaryReader(stream)) {
                        byte[] header = reader.ReadBytes(24);
                        c.Require(header.Length == 24 && header[0] == 137 && header[1] == 80 &&
                            header[2] == 78 && header[3] == 71, "Invalid PNG " + file);
                        int width = (header[16]<<24)|(header[17]<<16)|(header[18]<<8)|header[19];
                        int height = (header[20]<<24)|(header[21]<<16)|(header[22]<<8)|header[23];
                        c.Require(width > 0 && height > 0, "Empty PNG " + file);
                    }
                    return;
                }
            }
            c.Require(false, "Screenshot not written (hidden renderer may not draw): " + path);
        }
    }
}
