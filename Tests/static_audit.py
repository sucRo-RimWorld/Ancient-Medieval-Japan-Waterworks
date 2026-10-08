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
    "t.IsIce",
):
    assert required in source, required
assert 't.defName == "Marsh"' in source
assert 't.defName == "Mud"' in source
assert "!t.natural" not in component_source if "component_source" in globals() else "!t.natural" not in source
assert 'affordance.defName == "Diggable"' in component_source if "component_source" in globals() else 'affordance.defName == "Diggable"' in source
assert "TerrainAffordanceDefOf.Diggable" not in source
assert '!t.affordances.Exists(affordance => affordance != null && affordance.defName == "Diggable")' in source


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

# RimWorld 1.6 event invalidation: retain map index and avoid full-map polling.
component_source = (root / "Source/CanalMapComponent.cs").read_text(encoding="utf-8")
for marker in (
    "map.events.TerrainChanged += OnTerrainChanged;",
    "map.events.TerrainChanged -= OnTerrainChanged;",
    "RebuildCanalIndex();",
    "foreach (IntVec3 start in canalCells)",
    "if (needsRecalculation)",
    "changingCanalTerrain = true;",
    "changingCanalTerrain = false;",
):
    assert marker in component_source, marker
assert "TicksGame %" not in component_source, "Periodic full-map polling is not permitted"

# Natural ground can be designated through wild vegetation, but cultivated
# crops are never silently cut and terrain replacement waits for clearing.
assert "CanDig(IntVec3 c, bool allowWildPlants = false)" in component_source
assert "plant.sown || !allowWildPlants" in component_source
jobs_source = (root / "Source/CanalJobs.cs").read_text(encoding="utf-8")
designators_source = (root / "Source/CanalDesignators.cs").read_text(encoding="utf-8")
assert "JobMaker.MakeJob(JobDefOf.CutPlant, wildPlant)" in jobs_source
assert "wildPlant.sown || wildPlant.IsForbidden(pawn)" in jobs_source
assert "CanDig(c, allowWildPlants: true)" in designators_source
assert "public override bool HasJobOnCell" in jobs_source
assert "if (!HasJobOnCell(pawn, c, forced)) return null;" in jobs_source
assert "this.FailOn(() => !CanAffect(" in jobs_source
assert "component.CanDig(cell, allowWildPlants: true)" in jobs_source
assert "component.CanDig(cell);" in jobs_source
assert "component.CanFill(cell);" in jobs_source


assert "Scripts/" in (root / ".rimignore").read_text(encoding="utf-8")
# Pickle feature/step synchronization check; does not execute RimWorld.
feature = (root / "Tests/E2E/TestMod/Pickle/Features/waterworks-core.feature").read_text(encoding="utf-8")
e2e_steps = (root / "Tests/E2E/WaterworksSteps.cs").read_text(encoding="utf-8")
assert feature.count("  Scenario:") == 4
assert feature.count("@quickstart:WaterworksQuickstart") == 3
for step in (
    "Waterworks loaded Defs preserve the canal contract",
    "cardinal canal branches connect disconnect and reconnect",
    "standing ponds use the nine cell freshwater threshold",
    "Vanilla bridge preserves water and gravel restoration",
):
    assert "Then " + step in feature
    assert '[Then("' + step + '")]' in e2e_steps
assert "TestResults/" in (root / ".rimignore").read_text(encoding="utf-8")
assert (root / "Scripts/run-e2e.ps1").exists()
print("[OK] XML, source and E2E contracts checked (runtime not tested)")
