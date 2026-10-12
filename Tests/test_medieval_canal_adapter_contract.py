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


def test_no_false_claim_of_irrigation_implementation():
    assert "actual adapter and runtime acceptance remain unimplemented/unverified" in DESIGN


if __name__ == "__main__":
    test_natural_supply_not_inferred_from_foreign_pipes()
    test_only_cardinal_contact()
    test_no_false_claim_of_irrigation_implementation()
    print("PASS: source-only Medieval canal compatibility query guards (NOT gameplay integration)")
