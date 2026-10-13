using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimTestRedux;
using RimWorld;
using UnityEngine;
using Verse;
using AncientMedievalJapan.Waterworks;
namespace AMJW.Waterworks.RimTests
{
    [TestSuite]
    public static class CanalContracts
    {
        [Test]
        public static void StandingWaterClassifierExcludesOceanWetlandsAndCanals()
        {
            var method = AccessTools.Method(typeof(CanalMapComponent), "IsStanding");
            foreach (var name in new[] { "WaterShallow", "WaterDeep", "WaterOceanShallow", "Marsh", "Mud", "AMJW_DugCanalWet" })
            {
                bool actual = (bool)method.Invoke(null, new object[] { DefDatabase<TerrainDef>.GetNamed(name) });
                Assert.That(actual == (name == "WaterShallow" || name == "WaterDeep")).Is.True();
            }
        }
        [Test]
        public static void CanalDefsPreserveBridgeAndNaturalSourceBoundary()
        {
            foreach (var name in new[] { "AMJW_DugCanalDry", "AMJW_DugCanalWet" })
            {
                var def = DefDatabase<TerrainDef>.GetNamed(name);
                Assert.That(def.pathCost == 10 && !def.IsWater && !def.IsRiver && def.affordances.Contains(TerrainAffordanceDefOf.Bridgeable)).Is.True();
            }
        }
        private static int WrongReturn() { return 0; }
        private static bool WrongArguments(int value) { return true; }
        private static bool Valid() { return true; }
        [Test]
        public static void OptionalAdapterRejectsUnknownMethodSignatures()
        {
            var guard = AccessTools.Method(typeof(MedievalCanalCompatibility), "ReturnsBoolWithoutArguments");
            Assert.That(!(bool)guard.Invoke(null, new object[] { null })).Is.True();
            foreach (var name in new[] { "WrongReturn", "WrongArguments", "Valid" })
                Assert.That((bool)guard.Invoke(null, new object[] { AccessTools.Method(typeof(CanalContracts), name) }) == (name == "Valid")).Is.True();
        }
        [Test]
        public static void OptionalAdapterPatchesOnlyAvailablePartnerMethods()
        {
            var type = AccessTools.TypeByName("DBHforMedieval.CompSluiceGate");
            if (type == null)
            {
                Assert.That(DefDatabase<ThingDef>.GetNamedSilentFail("ES_IrrigationCanal") == null).Is.True();
                return;
            }
            foreach (var method in new[] { AccessTools.Method(type,"TrySpray"), AccessTools.Method(type,"IsWaitingForDoer"),
                AccessTools.PropertyGetter(AccessTools.TypeByName("DBHforMedieval.Building_IrrigationCanal"), "WaterFlow") })
                Assert.That(Harmony.GetPatchInfo(method).Owners.Contains("sucro.amjw.medievalcanal")).Is.True();
            var pipe = AccessTools.TypeByName("DubsBadHygiene.CompPipe");
            Assert.That(AccessTools.Property(pipe, "pipeNet") != null).Is.True();
            Assert.That(AccessTools.Field(AccessTools.TypeByName("DubsBadHygiene.PlumbingNet"), "WaterTowers") != null).Is.True();
            Assert.That(AccessTools.Field(AccessTools.TypeByName("DubsBadHygiene.CompWaterStorage"), "WaterStorage") != null).Is.True();
        }
    }
    [StaticConstructorOnStartup]
    public static class RimTestAutomationBridge
    {
        private const string AssemblyName = "AMJW.Waterworks.RimTests";

        static RimTestAutomationBridge()
        {
            try
            {
                Type viewer = AccessTools.TypeByName("RimTestRedux.Testing.Viewer");
                MethodInfo target = viewer == null ? null : AccessTools.Method(viewer, "LogTestsResults");
                if (target == null)
                    throw new InvalidOperationException("RimTest Redux Viewer.LogTestsResults was not found");
                new Harmony("sucro.amjw.rimtest.bridge").Patch(
                    target,
                    postfix: new HarmonyMethod(typeof(RimTestAutomationBridge), "AfterResultsLogged"));
            }
            catch (Exception error)
            {
                WriteFatal(error);
                Application.Quit();
            }
        }

        public static void AfterResultsLogged()
        {
            string path = System.Environment.GetEnvironmentVariable("AMJW_RIMTEST_RESULT");
            if (String.IsNullOrEmpty(path))
                return;
            try
            {
                Assembly testAssembly = typeof(CanalContracts).Assembly;
                Type assemblyLink = AccessTools.TypeByName("RimTestRedux.Testing.Assembly2TestSuiteLink");
                Type testLink = AccessTools.TypeByName("RimTestRedux.Testing.TestSuite2TestLink");
                Type explorer = AccessTools.TypeByName("RimTestRedux.Testing.TestExplorer");
                MethodInfo getSuites = AccessTools.Method(assemblyLink, "GetTestSuites");
                MethodInfo getTests = AccessTools.Method(testLink, "GetTests");
                MethodInfo getStatus = AccessTools.Method(explorer, "GetTestStatus");
                MethodInfo getError = AccessTools.Method(explorer, "GetTestError");
                if (getSuites == null || getTests == null || getStatus == null || getError == null)
                    throw new InvalidOperationException("RimTest Redux result API changed");

                var rows = new List<string>();
                int passed = 0, failed = 0, skipped = 0, unknown = 0;
                IEnumerable suites = (IEnumerable)getSuites.Invoke(null, new object[] { testAssembly });
                foreach (Type suite in suites.Cast<Type>().OrderBy(x => x.FullName))
                {
                    IEnumerable tests = (IEnumerable)getTests.Invoke(null, new object[] { suite });
                    foreach (MethodInfo test in tests.Cast<MethodInfo>().OrderBy(x => x.Name))
                    {
                        object statusObject = getStatus.Invoke(null, new object[] { test });
                        string status = statusObject == null ? "UNKNOWN" : statusObject.ToString();
                        Exception error = getError.Invoke(null, new object[] { test }) as Exception;
                        if (status == "PASS") passed++;
                        else if (status == "ERROR") failed++;
                        else if (status == "SKIP") skipped++;
                        else unknown++;
                        rows.Add("{\"suite\":\"" + Json(suite.Name) + "\",\"test\":\"" +
                                 Json(test.Name) + "\",\"status\":\"" + Json(status) +
                                 "\",\"error\":\"" + Json(error == null ? "" : error.ToString()) + "\"}");
                    }
                }
                int total = passed + failed + skipped + unknown;
                bool ok = total == 4 && failed == 0 && skipped == 0 && unknown == 0;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path,
                    "{\n  \"assembly\": \"" + AssemblyName + "\",\n" +
                    "  \"productionAssembly\": \"" + Json(typeof(CanalMapComponent).Assembly.Location) + "\",\n" +
                    "  \"passed\": " + (ok ? "true" : "false") + ",\n" +
                    "  \"total\": " + total + ",\n" +
                    "  \"pass\": " + passed + ",\n" +
                    "  \"failed\": " + failed + ",\n" +
                    "  \"skipped\": " + skipped + ",\n" +
                    "  \"unknown\": " + unknown + ",\n" +
                    "  \"tests\": [" + String.Join(",", rows.ToArray()) + "]\n}\n");
            }
            catch (Exception error)
            {
                WriteFatal(error);
            }
            finally
            {
                Application.Quit();
            }
        }

        private static void WriteFatal(Exception error)
        {
            string path = System.Environment.GetEnvironmentVariable("AMJW_RIMTEST_RESULT");
            if (String.IsNullOrEmpty(path))
                return;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, "{\"assembly\":\"" + AssemblyName +
                    "\",\"passed\":false,\"fatal\":\"" + Json(error.ToString()) + "\"}\n");
            }
            catch { }
        }

        private static string Json(string value)
        {
            if (value == null) return "";
            var b = new StringBuilder();
            foreach (char c in value)
            {
                switch (c)
                {
                    case '\\': b.Append("\\\\"); break;
                    case '"': b.Append("\\\""); break;
                    case '\r': b.Append("\\r"); break;
                    case '\n': b.Append("\\n"); break;
                    case '\t': b.Append("\\t"); break;
                    default:
                        if (c < 32) b.Append("\\u" + ((int)c).ToString("x4"));
                        else b.Append(c);
                        break;
                }
            }
            return b.ToString();
        }
    }
}
