#!/usr/bin/env python3
from __future__ import annotations

import importlib.util
from pathlib import Path
import subprocess
import sys
import tempfile

ROOT = Path(__file__).resolve().parents[1]
SCRIPT = ROOT / "Scripts" / "canal_tile_pipeline.py"

spec = importlib.util.spec_from_file_location("canal_tile_pipeline", SCRIPT)
assert spec and spec.loader
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)

with tempfile.TemporaryDirectory() as td:
    out = Path(td) / "EW"
    report = mod.build_and_validate("EW", out)
    assert report["status"] == "PASS", report
    assert report["productionApproved"] is False
    assert (out / "AMJW_Canal_Straight_EW_Dry.png").exists()
    assert (out / "AMJW_Canal_Straight_EW_Wet.png").exists()
    assert (out / "validation.json").exists()

    # Same inputs/code must reproduce exact output bytes.
    hashes1 = report["sha256"]
    report2 = mod.build_and_validate("EW", out)
    assert hashes1 == report2["sha256"], "candidate output is not deterministic"

# Unsupported topology must fail rather than letting an image/model invent it.
proc = subprocess.run(
    [sys.executable, str(SCRIPT), "--mask", "NS", "--out", str(ROOT / "TestResults" / "_blocked")],
    text=True,
    stdout=subprocess.PIPE,
    stderr=subprocess.PIPE,
)
assert proc.returncode != 0
assert "Only EW baseline generation is enabled" in (proc.stdout + proc.stderr)

proc = subprocess.run(
    [sys.executable, str(SCRIPT), "--all"],
    text=True,
    stdout=subprocess.PIPE,
    stderr=subprocess.PIPE,
)
assert proc.returncode != 0
assert "--all is blocked" in (proc.stdout + proc.stderr)

print("Canal tile pipeline tests: PASS")
