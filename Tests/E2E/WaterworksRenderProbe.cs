using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace AncientMedievalJapan.Waterworks.E2E
{
    /// <summary>
    /// Non-invasive RimWorld 1.6 runtime API inspection.
    /// Called only by the isolated visual Pickle scenario on the game thread.
    /// No reflection-based production renderer or Harmony patch is installed.
    /// </summary>
    internal static class WaterworksRenderProbe
    {
        private static readonly BindingFlags Flags =
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public static void Write(string outputDirectory, IntVec3 focus)
        {
            var result = new StringBuilder();
            result.AppendLine("AMJW visual rendering API probe");
            result.AppendLine("These are signatures from the loaded game; not proof of layer ordering.");
            Dump(result, typeof(MapDrawLayer));
            Dump(result, typeof(SectionLayer));
            Dump(result, typeof(Section));
            Dump(result, typeof(MapDrawer));
            Dump(result, typeof(SectionLayer_Terrain));
            Dump(result, typeof(SectionLayer_Dynamic));
            Dump(result, typeof(TerrainGrid));
            DumpCurrentLayers(result, focus);
            File.WriteAllText(Path.Combine(outputDirectory, "render-api.txt"), result.ToString());
        }

        private static void DumpCurrentLayers(StringBuilder sb, IntVec3 focus)
        {
            sb.AppendLine();
            sb.AppendLine("RUNTIME SECTION LAYER ORDER (loaded map only)");
            Map map = Find.CurrentMap;
            if (map == null || map.mapDrawer == null)
            {
                sb.AppendLine("No current map or map drawer.");
                return;
            }
            sb.AppendLine("FOCUS " + focus);
            Section section = map.mapDrawer.SectionAt(focus);
            if (section == null)
            {
                sb.AppendLine("SectionAt(map center) returned null.");
                return;
            }
            FieldInfo field = typeof(Section).GetField("layers",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var layers = field == null ? null :
                field.GetValue(section) as System.Collections.IEnumerable;
            if (layers == null)
            {
                sb.AppendLine("Section layers unavailable.");
                return;
            }
            int index = 0;
            foreach (object layer in layers)
            {
                sb.AppendLine("INDEX " + index + " " +
                    (layer == null ? "(null)" : layer.GetType().FullName));
                index++;
            }
        }

        private static void Dump(StringBuilder sb, Type type)
        {
            sb.AppendLine();
            sb.AppendLine("TYPE " + type.FullName);
            sb.AppendLine("BASE " + (type.BaseType == null ? "(none)" : type.BaseType.FullName));
            foreach (ConstructorInfo c in type.GetConstructors(Flags).OrderBy(x => x.ToString()))
                sb.AppendLine("CTOR " + c);
            foreach (FieldInfo f in type.GetFields(Flags).OrderBy(x => x.Name))
                sb.AppendLine("FIELD " + f.FieldType.FullName + " " + f.Name);
            foreach (PropertyInfo p in type.GetProperties(Flags).OrderBy(x => x.Name))
                sb.AppendLine("PROP " + p);
            foreach (MethodInfo m in type.GetMethods(Flags).OrderBy(x => x.Name))
            {
                if (m.IsSpecialName) continue;
                sb.AppendLine("METHOD " + m);
            }
        }
    }
}
