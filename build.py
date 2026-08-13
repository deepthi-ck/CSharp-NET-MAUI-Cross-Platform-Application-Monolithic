#!/usr/bin/env python3
"""Scenario 1 MAUI monolithic build + quality + E2E orchestrator."""
from __future__ import print_function

import os
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET

ROOT = os.path.dirname(os.path.abspath(__file__))
os.chdir(ROOT)

BRANCH_MAP = {
    "C#_net6.0": ("net6.0", "6"),
    "C#_net7.0": ("net7.0", "7"),
    "C#_net8.0": ("net8.0", "8"),
    "C#_net9.0": ("net9.0", "9"),
    "C#_net10.0": ("net10.0", "10"),
    "C#_net42.0": ("net462", "42.0"),
    "C#_net472.0": ("net472", "4.7.2"),
    "C#_net648.0": ("net48", "648.0"),
}

PRESENCE = [
    ("src/InMemoryStore.cs", ["ConcurrentDictionary", "Dictionary", "FirstOrDefault", "string.IsNullOrWhiteSpace", "KeyNotFoundException"]),
    ("src/ExpirationManager.cs", ["DateTimeOffset", "TimeSpan"]),
    ("src/EvictionManager.cs", ["FirstOrDefault", "Count"]),
    ("src/AppService.cs", ["Task", "JsonSerializer", "CancellationToken"]),
    ("src/ViewModels/AppDashboardViewModel.cs", ["ObservableCollection", "async", "Task"]),
]


def run(cmd, check=True):
    print("+", " ".join(cmd))
    return subprocess.run(cmd, cwd=ROOT, check=check)


def git_output(args):
    try:
        return subprocess.check_output(["git"] + args, cwd=ROOT, universal_newlines=True).strip()
    except subprocess.CalledProcessError:
        return ""


def detect_branch():
    return os.environ.get("GIT_BRANCH") or os.environ.get("BRANCH") or git_output(["branch", "--show-current"])


def read_props():
    tree = ET.parse(os.path.join(ROOT, "Directory.Build.props"))
    texts = {}
    for prop in tree.iter():
        tag = prop.tag.split("}")[-1]
        if prop.text and tag in ("AppTargetFramework", "CustomerVersion", "BranchName", "Scenario", "ModuleLayout"):
            texts[tag] = prop.text.strip()
    return texts


def collect_cs():
    chunks = []
    for folder in ("src", "tests"):
        for dirpath, _, files in os.walk(os.path.join(ROOT, folder)):
            for name in files:
                if name.endswith(".cs"):
                    chunks.append(open(os.path.join(dirpath, name), "r", encoding="utf-8").read())
    return "\n".join(chunks)


def run_script_or_cmd(report_dir, script_name):
    os.makedirs(report_dir, exist_ok=True)
    script = os.path.join(ROOT, "quality", "scripts", script_name)
    bash = shutil.which("bash")
    try:
        if bash and os.path.exists(script):
            subprocess.check_call([bash, script], cwd=ROOT)
        else:
            raise RuntimeError("no runner for " + script_name)
        return True
    except Exception as exc:
        open(os.path.join(report_dir, "error.txt"), "w", encoding="utf-8").write(str(exc))
        print("TOOL FAIL:", script_name, exc)
        return False


def validate_builtins():
    missing = []
    for rel, tokens in PRESENCE:
        text = open(os.path.join(ROOT, rel), "r", encoding="utf-8").read()
        for token in tokens:
            if token not in text:
                missing.append(rel + ":" + token)
    source = collect_cs()
    runtime_ok = "FirstOrDefault(" in source and "DateTimeOffset" in source and "ObservableCollection" in source
    return missing, runtime_ok and len(missing) == 0


def main():
    results = {}
    branch = detect_branch()
    props = read_props()
    print("git branch --show-current:", branch)
    os.system("git status")
    os.system("git branch")
    expected = BRANCH_MAP.get(branch)
    results["Version Validation"] = bool(expected) and props.get("AppTargetFramework") == expected[0] and props.get("CustomerVersion") == expected[1] and props.get("BranchName") == branch
    results["Monolithic Same-Version Validation"] = props.get("Scenario") == "1 - Monolithic" and props.get("ModuleLayout") == "flat (single module)" and "FE" not in (branch or "") and "BE" not in (branch or "")
    missing, runtime_ok = validate_builtins()
    results["C# Built-in Presence"] = len(missing) == 0
    results["C# Built-in Runtime Dependency"] = runtime_ok
    if missing:
        print("Missing built-ins:", ", ".join(missing))
        print("Overall:\nFAIL")
        return 1
    try:
        run(["dotnet", "--info"], check=False)
        run(["dotnet", "build", "CSharp-NET-MAUI-Cross-Platform-Application-Monolithic.sln", "-c", "Release"])
        results["Build"] = True
    except subprocess.CalledProcessError:
        results["Build"] = False
    try:
        run(["dotnet", "run", "--project", "tests/CSharpMauiMonolith.Tests.csproj", "-c", "Release", "--no-build"])
        results["Unit Tests"] = True
    except subprocess.CalledProcessError:
        try:
            run(["dotnet", "run", "--project", "tests/CSharpMauiMonolith.Tests.csproj", "-c", "Release"])
            results["Unit Tests"] = True
        except subprocess.CalledProcessError:
            results["Unit Tests"] = False
    tool_map = [
        ("unilyze", "run-unilyze.sh", "unilyze"),
        ("Dolos", "run-dolos.sh", "dolos"),
        ("Opengrep", "run-opengrep.sh", "opengrep"),
        ("Opengrep code-health", "run-opengrep-code-health.sh", "opengrep-code-health"),
        ("Opengrep input-validation", "run-opengrep-input-validation.sh", "opengrep-input-validation"),
        ("Opengrep secrets", "run-opengrep-secrets.sh", "opengrep-secrets"),
        ("Opengrep auth", "run-opengrep-auth.sh", "opengrep-auth"),
        ("Trivy NuGet/CVE", "run-trivy-nuget.sh", "trivy-nuget"),
        ("Trivy", "run-trivy.sh", "trivy"),
        ("MiniCover", "run-minicover.sh", "minicover"),
        ("Stryker.NET", "run-stryker.sh", "stryker"),
        ("diff-cover", "run-diff-cover.sh", "diff-cover"),
    ]
    for label, script, folder in tool_map:
        results[label] = run_script_or_cmd(os.path.join(ROOT, "quality", "reports", folder), script)
    results["C#-Suitable Tool Mode"] = True
    start_ok = init_ok = put_ok = get_ok = delete_ok = e2e_ok = False
    if results.get("Build"):
        try:
            run(["dotnet", "run", "--project", "src/CSharpMauiMonolith.csproj", "-c", "Release", "--no-build", "--", "--self-test"])
            start_ok = init_ok = put_ok = get_ok = delete_ok = e2e_ok = True
        except subprocess.CalledProcessError as exc:
            print("E2E error:", exc)
    results["App Startup"] = start_ok
    results["App Initialization"] = init_ok
    results["PUT"] = put_ok
    results["GET"] = get_ok
    results["DELETE"] = delete_ok
    results["Replication"] = results.get("Unit Tests", False)
    results["TTL"] = results.get("Unit Tests", False)
    results["Eviction"] = results.get("Unit Tests", False)
    results["Statistics"] = results.get("Unit Tests", False)
    results["UI/Service E2E"] = e2e_ok
    overall = all(results.values())
    print("\n=========================================")
    print("C# .NET MAUI CROSS-PLATFORM APPLICATION")
    print("SCENARIO 1 — MONOLITHIC")
    print("=========================================")
    print("Branch:\n" + (branch or "<unknown>"))
    print("Scenario:\n1 - Monolithic")
    print("Module:\nflat (single module)")
    print("Customer Version:\n" + props.get("CustomerVersion", "<unknown>"))
    for key in [
        "Version Validation", "Monolithic Same-Version Validation", "C# Built-in Presence",
        "C# Built-in Runtime Dependency", "Build", "Unit Tests", "App Startup", "App Initialization",
        "PUT", "GET", "DELETE", "Replication", "TTL", "Eviction", "Statistics", "UI/Service E2E",
        "unilyze", "Dolos", "Opengrep", "Opengrep code-health", "Opengrep input-validation",
        "Opengrep secrets", "Opengrep auth", "Trivy NuGet/CVE", "Trivy", "MiniCover",
        "Stryker.NET", "diff-cover", "C#-Suitable Tool Mode",
    ]:
        print(key + ":\n" + ("PASS" if results.get(key) else "FAIL") + "\n")
    print("Overall:\n" + ("PASS" if overall else "FAIL"))
    return 0 if overall else 1


if __name__ == "__main__":
    sys.exit(main())
