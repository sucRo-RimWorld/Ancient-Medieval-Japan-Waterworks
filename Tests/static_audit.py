#!/usr/bin/env python3
"""Offline XML/structural preflight; not a game-runtime test."""
from pathlib import Path
import xml.etree.ElementTree as ET
import re

root = Path(__file__).resolve().parents[1]
xmls = list(root.glob("About/*.xml")) + list(root.glob("Defs/**/*.xml")) + list(root.glob("Patches/*.xml")) + list(root.glob("Languages/**/*.xml"))
assert len(xmls) == 9, (len(xmls), [str(p) for p in xmls])
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
               "BaseWorkAmount => 300", "DrawStyleCategory => AMJW_Defs.AMJW_CanalLine"):
    assert marker in source, marker
draw_style = ET.parse(root / "Defs/DrawStyleCategoryDefs/AMJW_CanalLine.xml").getroot()
category = draw_style.find("DrawStyleCategoryDef")
assert category is not None
assert category.findtext("defName") == "AMJW_CanalLine"
assert [style.text for style in category.find("styles")] == ["Line"]
assert "public static DrawStyleCategoryDef AMJW_CanalLine;" in source
assert "DraggableDimensions" not in source, "RimWorld 1.6 removed this Designator API"
assert "protected override DesignationDef Designation => EarthworkDef;" in source

# The regression checks below validate references, not executable game behavior.
def class_exists(class_name):
    name = class_name.rsplit(".", 1)[-1]
    return re.search(r"\bclass\s+" + re.escape(name) + r"\b", source) is not None

for xml_path, field in (
    ("Defs/JobDefs/AMJW_Jobs.xml", "driverClass"),
    ("Defs/WorkGiverDefs/AMJW_WorkGivers.xml", "giverClass"),
):
    for node in ET.parse(root / xml_path).iter(field):
        assert class_exists(node.text), (xml_path, node.text)

orders = ET.parse(root / "Patches/AMJW_Orders.xml")
order_classes = [n.text for n in orders.iter("li")]
assert len(order_classes) == 2
for class_name in order_classes:
    assert class_exists(class_name), class_name

for def_name, kind in (
    ("AMJW_DugCanalDry", "TerrainDef"),
    ("AMJW_DugCanalWet", "TerrainDef"),
    ("AMJW_DigCanal", "DesignationDef"),
    ("AMJW_FillCanal", "DesignationDef"),
    ("AMJW_DigCanalJob", "JobDef"),
    ("AMJW_FillCanalJob", "JobDef"),
    ("AMJW_CanalLine", "DrawStyleCategoryDef"),
):
    declaration = "public static " + kind + " " + def_name + ";"
    assert declaration in source, declaration

for required in (
    "DrawStyleCategory => AMJW_Defs.AMJW_CanalLine",
    "Designation => EarthworkDef",
    "GenAdj.CardinalDirections",
    "seen.Count >= 9",
    "Scribe_Collections.Look(ref originals",
    "FoundationAt(c)",
    "originals.Remove(index)",
    "t.IsRoad",
    "t == TerrainDefOf.Ice",
):
    assert required in source, required
assert 't.defName == "Marsh"' in source
assert 't.defName == "Mud"' in source

localization_keys = (
    "AMJW_DigLabel", "AMJW_DigDesc", "AMJW_FillLabel", "AMJW_FillDesc",
    "AMJW_CannotDig", "AMJW_Blocked", "AMJW_RoadBlocked",
    "AMJW_AlreadyCanal", "AMJW_NotCanal", "AMJW_AlreadyDesignated",
    "AMJW_CannotRestore",
)
for language in ("English", "Japanese"):
    translated = ET.parse(root / "Languages" / language / "Keyed" / "AMJW.xml").getroot()
    for key in localization_keys:
        assert translated.find(key) is not None, (language, key)

all_defnames = []
for xml_path in (root / "Defs").rglob("*.xml"):
    all_defnames += [node.text for node in ET.parse(xml_path).iter("defName")]
assert len(all_defnames) == len(set(all_defnames)), "duplicate local DefName"

assert "Scripts/" in (root / ".rimignore").read_text(encoding="utf-8")
print("[OK] XML and RimWorld 1.6 draw-style contract checked (runtime not tested)")
