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
assert feature.count("  Scenario:") == 6
assert feature.count("@quickstart:WaterworksQuickstart") == 5
for step in (
    "Waterworks loaded Defs preserve the canal contract",
    "cardinal canal branches connect disconnect and reconnect",
    "standing ponds use the nine cell freshwater threshold",
    "Vanilla bridge preserves water and gravel restoration",
    "a construction pawn actually digs and fills a canal",
    "Waterworks rebuilds supply and restores both saved ground types",
):
    assert "Then " + step in feature
    assert '[Then("' + step + '")]' in (
        e2e_steps + (root / "Tests/E2E/WaterworksPawnJobs.cs").read_text(encoding="utf-8") +
        (root / "Tests/E2E/WaterworksPersistenceSteps.cs").read_text(encoding="utf-8")
    )
assert "TestResults/" in (root / ".rimignore").read_text(encoding="utf-8")
assert (root / "Scripts/run-e2e.ps1").exists()
assert '<Compile Include="WaterworksPawnJobs.cs"/>' in (
    root / "Tests/E2E/Steps.csproj").read_text(encoding="utf-8")
pawn_source = (root / "Tests/E2E/WaterworksPawnJobs.cs").read_text(encoding="utf-8")
for required in (
    "WorkGiver_Scanner",
    "giver.JobOnCell(worker, target, true)",
    "worker.jobs.StartJob(job, JobCondition.InterruptForced)",
    "Find.TickManager.DoSingleTick()",
    "AMJW_DigCanalJob",
    "AMJW_FillCanalJob",
):
    assert required in pawn_source, required

# Pickle supplies the real disk round-trip step; do not replace it with
# in-process MapComponent serialization (which would miss load events).
assert 'When I save and reload' in feature
assert 'Given Waterworks has supplied and dry canals with distinct original ground' in feature
persist_source = (root / "Tests/E2E/WaterworksPersistenceSteps.cs").read_text(encoding="utf-8")
assert '[Given("Waterworks has supplied and dry canals with distinct original ground")]' in persist_source
for expected in (
    'loadedMap.GetComponent<CanalMapComponent>()',
    'loaded.CanFill(suppliedCell).Accepted',
    'loaded.CanFill(dryCell).Accepted',
    'loaded.Fill(suppliedCell)',
    'loaded.Fill(dryCell)',
    'AMJW_DugCanalWet',
    'AMJW_DugCanalDry',
):
    assert expected in persist_source, expected
assert '<Compile Include="WaterworksPersistenceSteps.cs"/>' in (
    root / "Tests/E2E/Steps.csproj").read_text(encoding="utf-8")

# Existing-save installation is deliberately two separate game processes.
# Bootstrap creates an actual .rws with the Waterworks package ID absent,
# and the second run loads it with production Waterworks enabled.
bootstrap_feature = (root / "Tests/E2E/BootstrapMod/Pickle/Features/waterworks-before-install.feature").read_text(encoding="utf-8")
added_feature = (root / "Tests/E2E/AddToSaveMod/Pickle/Features/waterworks-add-to-save.feature").read_text(encoding="utf-8")
bootstrap_source = (root / "Tests/E2E/WaterworksBootstrapSteps.cs").read_text(encoding="utf-8")
add_source = (root / "Tests/E2E/WaterworksAddToSaveSteps.cs").read_text(encoding="utf-8")
bootstrap_about = ET.parse(root / "Tests/E2E/BootstrapMod/About/About.xml").getroot()
enabled_about = ET.parse(root / "Tests/E2E/AddToSaveMod/About/About.xml").getroot()
assert bootstrap_about.findtext("packageId") == "sucro.ancientmedievaljapan.waterworks.bootstrap"
assert enabled_about.findtext("packageId") == "sucro.ancientmedievaljapan.waterworks.addtosave"
assert not any(node.text == "sucro.ancientmedievaljapan.waterworks"
               for node in bootstrap_about.iter("packageId"))
assert any(node.text == "sucro.ancientmedievaljapan.waterworks"
           for node in enabled_about.iter("packageId"))
assert bootstrap_feature.count("  Scenario:") == 1
# A Pickle-only fixture causes RimWorld 1.6 to log "did not load any content".
# The test-specific Def is inert gameplay-wise but must be staged by the runner.
marker_path = root / "Tests/E2E/AddToSaveMod/Defs/ThingCategoryDefs/AMJW_E2E_Marker.xml"
marker_tree = ET.parse(marker_path).getroot()
assert marker_tree.find("ThingCategoryDef/defName").text == "AMJW_E2E_AddToSaveMarker"
assert "'Defs/ThingCategoryDefs'" in (root / "Scripts/run-add-to-save-e2e.ps1").read_text(encoding="utf-8")
assert "Tests/E2E/AddToSaveMod/Defs/ThingCategoryDefs/AMJW_E2E_Marker.xml" in (root / "Scripts/run-add-to-save-e2e.ps1").read_text(encoding="utf-8")
assert added_feature.count("  Scenario:") == 1
assert '@quickstart:WaterworksQuickstart' in bootstrap_feature
assert 'When I save and reload as "waterworks-before-install"' in bootstrap_feature
assert 'Given the save file "waterworks-before-install" is loaded' in added_feature
assert 'When I save and reload' in added_feature
assert '[Then("the Vanilla save baseline is prepared without Waterworks")]' in bootstrap_source
assert '[Then("Waterworks first loads without changing Vanilla ground and can dig canals")]' in add_source
assert '[Then("Waterworks persists and restores old Vanilla soil and gravel")]' in add_source
assert 'AncientMedievalJapan.Waterworks;' not in bootstrap_source
assert 'DefDatabase<TerrainDef>.GetNamedSilentFail("AMJW_DugCanalDry") == null' in bootstrap_source
assert '<Compile Include="WaterworksAddToSaveSteps.cs"/>' in (
    root / "Tests/E2E/Steps.csproj").read_text(encoding="utf-8")
assert '<Compile Include="WaterworksBootstrapSteps.cs"/>' in (
    root / "Tests/E2E/BootstrapSteps.csproj").read_text(encoding="utf-8")
runner = (root / "Scripts/run-add-to-save-e2e.ps1").read_text(encoding="utf-8")
for marker in ('Write-IsolatedConfig $false', 'Write-IsolatedConfig $true',
               "waterworks-before-install.feature", "waterworks-add-to-save.feature",
               "Saves/waterworks-before-install.rws", "TestResults/AddToSave",
               '([int]$summary.total -eq 1)', '([int]$summary.passed -eq 1)'):
    assert marker in runner, marker
assert 'TestResults/E2E/SaveData' not in runner
# A previous release contained an unbraced interpolated '$phase:' causing
# ParserError before any of the E2E tests could start.
assert 'Write-Host "[OK] ${phase}: 1/1 Pickle scenario, zero runtime ERROR."' in runner
assert 'Write-Host "[OK] $phase: 1/1 Pickle scenario, zero runtime ERROR."' not in runner

# The local validator must use PowerShell's own parser for every script so
# future parser regressions are caught before any RimWorld launch.
powershell_validator = (root / "Scripts/validate-source.ps1").read_text(encoding="utf-8")
assert '[System.Management.Automation.Language.Parser]::ParseFile' in powershell_validator
assert "-Filter '*.ps1'" in powershell_validator
# The runtime gate must expose actual ERROR lines and stack context, not
# only the outer throw site, and must never accept an ERROR as a PASS.
for marker in (
    "Select-String -LiteralPath $log -Pattern",
    "-Context 0,12",
    'Write-Host ("[RUNTIME-ERROR] " + $entry.Line)',
    'Write-Host ("[RUNTIME-CONTEXT] " + $line)',
    'if ($errors -gt 0) {',
):
    assert marker in runner, marker



# Visual evidence runner is separate from the accepted core 6/6 suite.
visual_feature = (root / "Tests/E2E/TestMod/Pickle/Features/waterworks-visual.feature").read_text(encoding="utf-8")
visual_steps = (root / "Tests/E2E/WaterworksVisualSteps.cs").read_text(encoding="utf-8")
visual_runner = (root / "Scripts/run-visual-e2e.ps1").read_text(encoding="utf-8")
visual_csproj = (root / "Tests/E2E/Steps.csproj").read_text(encoding="utf-8")
assert visual_feature.count("  Scenario:") == 1
assert "Then Waterworks renders the connected disconnected and restored canal scene" in visual_feature
assert '[Then("Waterworks renders the connected disconnected and restored canal scene")]' in visual_steps
assert '<Compile Include="WaterworksVisualSteps.cs"/>' in visual_csproj
assert 'UnityEngine.ScreenCaptureModule.dll' in visual_csproj
for token in ("ScreenCapture.CaptureScreenshot", "connected.png", "disconnected.png",
              "restored.png", "TerrainDefOf.Bridge", "Find.CameraDriver.JumpToCurrentMapLoc",
              "new FileInfo(path).Length > 24"):
    assert token in visual_steps, token
for token in ("waterworks-visual.feature", "TestResults/Visual",
              "Select-String -LiteralPath $log -Pattern", "Screenshot missing:",
              "WaterworksVisual"):
    assert token in visual_runner, token
assert "waterworks-core.feature" not in visual_runner
assert "run-e2e.ps1" not in visual_runner
assert visual_steps.count("await Task.Delay(1500);") == 3, "Each screenshot must wait for a rendered frame after state changes"
assert "int bestDistance = int.MaxValue;" in visual_steps, "Visual fixture should prefer map center"
ET.parse(root / "Tests/E2E/VisualMod/About/About.xml")
ET.parse(root / "Tests/E2E/VisualMod/Defs/ThingCategoryDefs/AMJW_VisualMarker.xml")

# Render geometry is a pure cardinal-mask model; no changes to water graph.
visual_geometry = (root / "Source/CanalVisualTopology.cs").read_text(encoding="utf-8")
for token in ("public const int North = 1", "public const int East = 2",
              "public const int South = 4", "public const int West = 8",
              "HalfChannel = 0.20f", "HalfBank = 0.34f",
              "public static int Mask(Map map, IntVec3 cell)",
              "public static Rectangle[] Rectangles(int mask, float halfWidth)",
              "terrain == AMJW_Defs.AMJW_DugCanalWet",
              "terrain == AMJW_Defs.AMJW_DugCanalDry"):
    assert token in visual_geometry, token
# Independently verify geometric specification for all 16 masks:
# core + one arm per cardinal neighbor, with adjoining edge extents.
for mask in range(16):
    half = 0.20
    rectangles = [(-half, -half, half, half)]
    if mask & 1: rectangles.append((-half, half, half, 0.5))
    if mask & 2: rectangles.append((half, -half, 0.5, half))
    if mask & 4: rectangles.append((-half, -0.5, half, -half))
    if mask & 8: rectangles.append((-0.5, -half, -half, half))
    assert len(rectangles) == 1 + bin(mask).count("1")
    for x0,z0,x1,z1 in rectangles:
        assert -0.5 <= x0 < x1 <= 0.5 and -0.5 <= z0 < z1 <= 0.5
    # Edge exits appear if and only if their cardinal mask bit is set.
    exits = {1: any(z1 == 0.5 for _,_,_,z1 in rectangles),
             2: any(x1 == 0.5 for _,_,x1,_ in rectangles),
             4: any(z0 == -0.5 for _,z0,_,_ in rectangles),
             8: any(x0 == -0.5 for x0,_,_,_ in rectangles)}
    for bit, present in exits.items():
        assert present == bool(mask & bit), (mask, bit)

# The isolated visual runner must obtain loaded-game draw-layer signatures
# before a production SectionLayer integration is designed.
render_probe = (root / "Tests/E2E/WaterworksRenderProbe.cs").read_text(encoding="utf-8")
assert '<Compile Include="WaterworksRenderProbe.cs"/>' in visual_csproj
assert "WaterworksRenderProbe.Write(output, center);" in visual_steps
assert "render-api.txt" in render_probe and "render-api.txt" in visual_runner
assert "RUNTIME SECTION LAYER ORDER" in render_probe
assert "SectionAt(focus)" in render_probe
assert "typeof(MapDrawLayer)" in render_probe
assert "typeof(LayerSubMesh)" in render_probe
assert "typeof(SectionLayer_Watergen)" in render_probe
assert "typeof(RimWorld.SectionLayer_BridgeProps)" in render_probe
assert "virtual=" in render_probe
for target in ("typeof(SectionLayer)", "typeof(Section)", "typeof(MapDrawer)",
               "typeof(SectionLayer_Terrain)", "typeof(TerrainGrid)"):
    assert target in render_probe, target

print("[OK] XML, source and E2E contracts checked (runtime not tested)")
