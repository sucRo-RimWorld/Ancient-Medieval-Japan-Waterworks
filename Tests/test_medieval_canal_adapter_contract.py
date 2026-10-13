"""Source-only checks for optional DBH for Medieval irrigation interoperability.

This is NOT a runtime or PipeNet supply test.
"""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
SOURCE = (ROOT / "Source" / "CanalMapComponent.cs").read_text(encoding="utf-8")
DESIGN = (ROOT / "Docs" / "Design.md").read_text(encoding="utf-8")


def method_body(name: str) -> str:
    match = re.search(r"public bool " + re.escape(name) + r"\([^)]*\)\s*\{", SOURCE)
    assert match is not None, f"missing {name}"
    offset = match.end()
    depth = 1
    for index in range(offset, len(SOURCE)):
        depth += (SOURCE[index] == "{") - (SOURCE[index] == "}")
        if depth == 0:
            return SOURCE[offset:index]
    raise AssertionError(f"unclosed body: {name}")


def test_natural_supply_not_inferred_from_foreign_pipes():
    body = method_body("IsSuppliedCanalAt")
    assert "needsRecalculation" in body and "Recalculate()" in body
    assert "AMJW_DugCanalWet" in body
    assert "CompPipe" not in body and "PipeNet" not in body


def test_only_cardinal_contact():
    body = method_body("TouchesSuppliedCanal")
    assert "GenAdj.CardinalDirections" in body
    assert "IsSuppliedCanalAt(cell + direction)" in body
    assert "Diagonal" not in body


def test_connected_medieval_canal_chain_is_only_a_geometry_query():
    body = method_body("MedievalIrrigationCanalTouchesWaterworksSource")
    assert "HasMedievalCanalAt(start)" in body
    assert "TouchesSuppliedCanal(at)" in body
    assert "GenAdj.CardinalDirections" in body
    assert "HasMedievalCanalAt(adjacent)" in body
    assert "PipeNet" not in body and "PushWater" not in body


def test_optional_adapter_contract():
    adapter = (ROOT / "Source" / "MedievalCanalCompatibility.cs").read_text(encoding="utf-8")
    assert "DBHforMedieval.CompSluiceGate" in adapter
    assert "DBHforMedieval.IrrigationUtility" in adapter
    assert "nameof(WaitingPostfix)" in adapter
    assert "nameof(SprayPrefix)" in adapter
    assert "nameof(FlowPrefix)" in adapter
    assert "nameof(FlowPostfix)" in adapter
    assert 'networkProperty.GetValue(pipe, null) == null' in adapter
    assert "AddMoistureMethod.Invoke" in adapter
    assert "DriedCellsMethod.Invoke" in adapter
    assert "ReturnsBoolWithoutArguments(wait)" in adapter
    assert "ReturnsBoolWithoutArguments(spray)" in adapter
    assert "ReturnsBoolWithoutArguments(flow)" in adapter
    assert "MatchesIrrigationMethods()" in adapter
    assert "AddMoistureMethod.ReturnType != typeof(void)" in adapter
    assert "CoolingMethod.ReturnType != typeof(void)" in adapter
    assert "CoolingMethod.GetParameters().Length != 0" in adapter
    assert 'AccessTools.Field(SluiceType, "width") == null' in adapter
    assert 'AccessTools.Field(SluiceType, "length") == null' in adapter
    assert "CanUseNativeStorage(__instance)" in adapter
    assert 'AccessTools.Field(tower.GetType(), "WaterStorage")' in adapter
    assert "available >= usage" in adapter
    assert "WaterTowers.Add" not in adapter
    assert "WaterStorage +=" not in adapter
    assert "loaded-game behavior remains unverified" in DESIGN


if __name__ == "__main__":
    test_natural_supply_not_inferred_from_foreign_pipes()
    test_only_cardinal_contact()
    test_connected_medieval_canal_chain_is_only_a_geometry_query()
    test_optional_adapter_contract()
    print("PASS: source-only Medieval canal compatibility query guards (NOT gameplay integration)")
