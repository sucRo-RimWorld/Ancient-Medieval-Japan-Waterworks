using RimWorks.Quickstarts;
using Verse;
namespace AncientMedievalJapan.Waterworks.E2E
{
    public sealed class WaterworksQuickstart : AbstractQuickstart
    {
        public override TaggedString description { get { return "Waterworks canal E2E test map."; } }
        public override int mapSize { get { return 50; } }
        public override string seed { get { return "AMJ-Waterworks-E2E"; } }
    }
}
