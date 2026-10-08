#!/usr/bin/env python3
"""Offline XML/structural preflight; not a game-runtime test."""
from pathlib import Path
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
xmls = list(root.glob("About/*.xml")) + list(root.glob("Defs/**/*.xml")) + list(root.glob("Patches/*.xml")) + list(root.glob("Languages/**/*.xml"))
assert len(xmls) == 8, (len(xmls), [str(p) for p in xmls])
for path in xmls:
    ET.parse(path)
about = ET.parse(root / "About/About.xml").getroot()
assert about.findtext("packageId") == "sucro.ancientmedievaljapan.waterworks"
assert about.find("supportedVersions").findtext("li") == "1.6"
terrains = ET.parse(root / "Defs/TerrainDefs/AMJW_Canals.xml").getroot()
defs = {t.findtext("defName"): t for t in terrains.findall("TerrainDef")}
assert set(defs) == {"AMJW_DugCanalDry", "AMJW_DugCanalWet"}
for td in defs.values():
    assert td.findtext("pathCost") == "10"
    assert td.find("affordances").findtext("li") == "Bridgeable"
    assert [n.text for n in td.find("tags")] == ["AMJW_Canal"]
assert defs["AMJW_DugCanalWet"].findtext("texturePath") == "Terrain/Surfaces/WaterShallowRamp"
assert defs["AMJW_DugCanalWet"].findtext("waterDepthShader") == "Map/WaterDepth"
assert defs["AMJW_DugCanalDry"].findtext("texturePath") == "Terrain/Surfaces/Soil"
assert not any(elem.find("holdSnow") is not None for elem in defs.values())
assert defs["AMJW_DugCanalWet"].findtext("holdSnowOrSand") == "false"
source = "\n".join(p.read_text(encoding="utf-8") for p in (root / "Source").glob("*.cs"))
for marker in ("WaterMovingShallow", "WaterMovingChestDeep", "WaterShallow", "WaterDeep",
               "seen.Count >= 9", "GenAdj.CardinalDirections", "FoundationAt(c)",
               "LookMode.Value, LookMode.Value", "BaseWorkAmount => 500",
               "BaseWorkAmount => 300", "DraggableDimensions => 1"):
    assert marker in source, marker
print("[OK] XML and contract invariants verified (runtime not tested)")
