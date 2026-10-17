#!/usr/bin/env python3
"""Explicitly triggered native Waterworks 1.6 Pickle acceptance and post-test DLL build."""
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import signal
import subprocess
import tempfile
import xml.etree.ElementTree as ET

BASE = "f243dee031b02ff2fd065d2296839670387bfe64"
MODID = "sucro.ancientmedievaljapan.waterworks"
TESTID = MODID + ".e2e"
SCENARIOS = {
    "Waterworks production Defs load correctly",
    "Four-direction canal branches connect and disconnect",
    "Canal width stays one cell",
    "Standing freshwater requires nine adjacent cells",
    "Mud and Marsh allow excavation without becoming freshwater sources",
    "Vanilla bridge preserves canal flow and terrain restoration",
    "Construction pawn completes real dig and fill jobs",
    "Save and reload restores canal supply and original ground",
}

def check(value, message):
    if not value:
        raise RuntimeError(message)

def modid(path):
    return (ET.parse(path / "About/About.xml").getroot().findtext("packageId") or "").lower()

def one_dll(folder, filename):
    """Resolve the active RimWorld 1.6 assembly rather than another version."""
    found = sorted(p for p in folder.rglob(filename) if p.is_file() and not p.is_symlink())
    native16 = [p for p in found if "1.6" in p.relative_to(folder).parts]
    if len(native16) == 1:
        return native16[0]
    # The provider may publish a non-versioned single-assembly distribution.
    if len(found) == 1:
        return found[0]
    check(False, "RimWorld 1.6 assembly ambiguous: " +
          ", ".join(p.relative_to(folder).as_posix() for p in found))
    raise AssertionError("unreachable")

def compile_project(project, output, env, substitutions=None, managed=None):
    tmp = project.with_name("AMJWNative_" + project.name)
    if substitutions:
        check(not tmp.exists(), "Generated project collision")
        text = project.read_text(encoding="utf-8")
        for before, after in substitutions.items():
            check(before in text, "Project reference changed: " + before)
            text = text.replace(before, str(after))
        tmp.write_text(text, encoding="utf-8")
    try:
        cmd = [str(Path.home()/".dotnet/dotnet"), "build",
               str(tmp if substitutions else project), "--nologo", "-c", "Release",
               "-p:NuGetAudit=false", "--output", str(output)]
        if managed:
            cmd.append("-p:RimWorldManagedDir=" + str(managed))
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=420, env=env)
        check(result.returncode == 0, "Build failed " + project.name + ": " +
              (result.stdout + result.stderr)[-5000:])
    finally:
        if substitutions:
            tmp.unlink(missing_ok=True)

def active_config(save, version):
    conf = save/"Config"
    conf.mkdir(parents=True)
    xml = ET.Element("ModsConfigData")
    ET.SubElement(xml, "version").text = version
    active = ET.SubElement(xml, "activeMods")
    for name in ("brrainz.harmony", "ludeon.rimworld", MODID,
                 "rimworks.rimlogging", "rimworks.pickle",
                 "rimworks.quickstarts", TESTID):
        ET.SubElement(active, "li").text = name
    ET.SubElement(xml, "knownExpansions")
    ET.ElementTree(xml).write(conf/"ModsConfig.xml", encoding="utf-8", xml_declaration=True)
    (conf/"Prefs.xml").write_text(
        "<PrefsData><devMode>True</devMode><resetModsConfigOnCrash>False</resetModsConfigOnCrash></PrefsData>",
        encoding="utf-8")

def run_native(game, work, out):
    save = work/"save"
    active_config(save, (game/"Version.txt").read_text().strip())
    log = work/"unity.log"
    launcher = work/"launcher.log"
    reports = work/"reports"
    env = os.environ.copy()
    home = work/"home"
    home.mkdir()
    env.update(HOME=str(home), XDG_CONFIG_HOME=str(home/".config"),
               XDG_DATA_HOME=str(home/".local/share"))
    env.pop("STEAM_COMPAT_DATA_PATH", None)
    command = [
        "xvfb-run", "-a", "-s", "-screen 0 1280x720x24",
        str(game/"RimWorldLinux"), "-savedatafolder=" + str(save),
        "-logFile", str(log), "-screen-width", "1280", "-screen-height", "720",
        "-pickle-run=waterworks-core.feature", "-pickle-mode=fast",
        "-pickle-report-dir=" + str(reports), "-pickle-run-timeout=10",
        "-pickle-no-browser", "-pickle-no-http", "-pickle-max-film-seconds=0"]
    proc = None
    try:
        with launcher.open("wb") as handle:
            proc = subprocess.Popen(command, cwd=game, env=env, stdout=handle,
                                    stderr=subprocess.STDOUT, start_new_session=True)
            try:
                proc.wait(timeout=900)
            except subprocess.TimeoutExpired:
                os.killpg(proc.pid, signal.SIGKILL)
                proc.wait(timeout=15)
                raise RuntimeError("Native 1.6 Pickle timeout")
    finally:
        for source, name in ((log, "unity-player.log"), (launcher, "launcher.log"),
                             (reports/"summary.json", "summary.json")):
            if source.is_file():
                (out/name).write_bytes(source.read_bytes()[-1000000:])
    check(proc is not None and proc.returncode == 0, "Native game failed to exit cleanly")
    check(log.is_file(), "Missing real Unity log")
    errors = re.findall(r"(?im)^.*\[ERROR\].*$", log.read_text(errors="replace"))
    check(len(errors) == 0, "Runtime ERROR: " + str(len(errors)))
    check((reports/"summary.json").is_file(), "Missing Pickle summary")
    summary = json.loads((reports/"summary.json").read_text(encoding="utf-8-sig"))
    scenarios = {row.get("name") for row in summary.get("scenarios", [])}
    check(scenarios == SCENARIOS and summary.get("total") == 8
          and summary.get("passed") == 8 and summary.get("failed") == 0
          and summary.get("skipped") == 0 and summary.get("flaky", 0) == 0
          and summary.get("exitReason") == "passed", "Eight required core Pickle cases not PASS")
    return {"source_sha": BASE, "game_version": (game/"Version.txt").read_text().strip(),
            "native_error_count": len(errors), "passed": 8,
            "scenario_names": sorted(scenarios), "native_exit": proc.returncode}

def main():
    source = Path.cwd()
    check(subprocess.run(["git", "diff", "--quiet", BASE, "HEAD", "--",
                          "Source", "Defs", "Patches", "Tests/E2E", "Tests/static_audit.py"],
                         cwd=source).returncode == 0,
          "Gameplay or test sources differ from the approved current main")
    root = Path("/srv/amj-rimworld")
    game = root/"game"
    mods = game/"Mods"
    version = (game/"Version.txt").read_text().strip()
    check(version.startswith("1.6."), "Real Linux 1.6 installation not available")
    managed = game/"RimWorldLinux_Data/Managed"
    check((managed/"Assembly-CSharp.dll").is_file(), "Real game references absent")
    check((Path.home()/".dotnet/dotnet").is_file(), "Existing dotnet compiler absent")
    old = mods/"AMJ_Waterworks"
    fixture = mods/"AncientMedievalJapanWaterworks.E2E"
    check(old.is_dir() and not old.is_symlink()
          and (old/".amj-test-stage.json").is_file()
          and modid(old) == MODID, "Original staged Waterworks not owned")
    check(not fixture.exists() and not fixture.is_symlink(), "Test fixture collision")
    for name, package in (("Harmony", "brrainz.harmony"),
                          ("RimLogging", "rimworks.rimlogging"),
                          ("Pickle", "rimworks.pickle"),
                          ("Quickstarts", "rimworks.quickstarts")):
        path = mods/("Framework_" + name)
        check(path.is_dir() and modid(path) == package, "Framework missing " + name)
    pickle = one_dll(mods/"Framework_Pickle", "RimWorks.Pickle.dll")
    quickstarts = one_dll(mods/"Framework_Quickstarts", "Quickstarts.dll")
    out = Path(os.environ["RUNNER_TEMP"])/"amjw-covered-intake16"
    out.mkdir(parents=True, exist_ok=True)
    env = os.environ.copy()
    env.update(DOTNET_CLI_TELEMETRY_OPTOUT="1", DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1")
    with tempfile.TemporaryDirectory(prefix="waterworks-covered16-", dir=root/"runs") as scratch:
        work = Path(scratch)
        env["DOTNET_CLI_HOME"] = str(work/"dotnet-home")
        env["NUGET_PACKAGES"] = str(work/"nuget-packages")
        candidate = work/"candidate"
        compile_project(source/"Source/AncientMedievalJapanWaterworks.csproj",
                        candidate, env, managed=managed)
        dll = candidate/"AncientMedievalJapanWaterworks.dll"
        check(dll.is_file() and dll.read_bytes()[:2] == b"MZ", "Candidate product invalid")
        tests = source/"Tests/E2E"
        quickout, stepsout = work/"quickout", work/"stepsout"
        compile_project(tests/"Quickstart.csproj", quickout, env, {
            "$(RimWorldDir)/RimWorldWin64_Data/Managed": managed,
            "$(QuickstartsDll)": quickstarts})
        compile_project(tests/"Steps.csproj", stepsout, env, {
            "$(RimWorldDir)/RimWorldWin64_Data/Managed": managed,
            "../../Assemblies/AncientMedievalJapanWaterworks.dll": dll,
            "$(PickleDll)": pickle})
        qb = quickout/"AncientMedievalJapanWaterworks.E2E.Quickstart.dll"
        sb = stepsout/"AncientMedievalJapanWaterworks.E2E.Steps.dll"
        check(qb.is_file() and sb.is_file(), "Compiled E2E binaries missing")
        backup = work/"original-stage"
        swapped = False
        try:
            old.rename(backup)
            swapped = True
            for folder in ("About", "Defs", "Patches", "Languages", "Textures", "Assemblies"):
                if (source/folder).is_dir():
                    shutil.copytree(source/folder, old/folder)
            shutil.copy2(dll, old/"Assemblies/AncientMedievalJapanWaterworks.dll")
            check(modid(old) == MODID, "Candidate package mismatch")
            for name in ("About", "Assemblies", "Pickle/Features", "Pickle/Assemblies"):
                (fixture/name).mkdir(parents=True, exist_ok=True)
            shutil.copy2(tests/"TestMod/About/About.xml", fixture/"About/About.xml")
            shutil.copy2(tests/"TestMod/Pickle/Features/waterworks-core.feature",
                         fixture/"Pickle/Features/waterworks-core.feature")
            shutil.copy2(qb, fixture/"Assemblies")
            shutil.copy2(sb, fixture/"Pickle/Assemblies")
            result = run_native(game, work, out)
        finally:
            if fixture.exists():
                check(modid(fixture) == TESTID, "Unexpected test fixture; refuse removal")
                shutil.rmtree(fixture)
            if swapped:
                if old.exists():
                    check(modid(old) == MODID, "Unexpected staged Mod; refuse removal")
                    shutil.rmtree(old)
                backup.rename(old)
        check(old.is_dir() and (old/".amj-test-stage.json").is_file(),
              "Original staged Waterworks not restored")
        # A fresh, final production build is deliberately after loaded-game PASS.
        final = work/"final"
        compile_project(source/"Source/AncientMedievalJapanWaterworks.csproj",
                        final, env, managed=managed)
        product = final/"AncientMedievalJapanWaterworks.dll"
        check(product.is_file() and product.read_bytes()[:2] == b"MZ",
              "Post-acceptance production build failed")
        deployed = source/"Assemblies/AncientMedievalJapanWaterworks.dll"
        shutil.copy2(product, deployed)
        result["release_sha256"] = hashlib.sha256(deployed.read_bytes()).hexdigest()
        result["post_test_build"] = True
        (out/"acceptance.json").write_text(json.dumps(result, indent=2) + "\n")
        print("AMJW_COVERED_NATIVE16_PASS " + result["release_sha256"], flush=True)

if __name__ == "__main__":
    main()
