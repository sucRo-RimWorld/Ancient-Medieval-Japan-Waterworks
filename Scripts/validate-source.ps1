# Non-interactive static and compile preflight. Does not launch RimWorld.
[CmdletBinding()]
param(
    [string]$RimWorldDir = $env:RIMWORLD_DIR
)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$python = Get-Command python -ErrorAction SilentlyContinue
if (-not $python) { throw "Python 3 is required for the Waterworks static audit." }
& $python.Source (Join-Path $root "Tests/static_audit.py")
if ($LASTEXITCODE -ne 0) { throw "Waterworks static audit failed (exit $LASTEXITCODE)." }

if (-not $RimWorldDir) {
    throw "Specify -RimWorldDir or set RIMWORLD_DIR. C# build did not run."
}
$assembly = Join-Path $RimWorldDir "RimWorldWin64_Data/Managed/Assembly-CSharp.dll"
if (-not (Test-Path $assembly)) { throw "RimWorld Assembly-CSharp.dll not found: $assembly" }
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) { throw ".NET SDK and net472 targeting pack are required." }
& $dotnet.Source build (Join-Path $root "Source/AncientMedievalJapanWaterworks.csproj") -c Release "-p:RimWorldDir=$RimWorldDir"
if ($LASTEXITCODE -ne 0) { throw "Waterworks C# build failed (exit $LASTEXITCODE)." }

Write-Output "[OK] Waterworks static audit and C# build completed."
Write-Warning "Pickle/RimTest Redux, runtime ERROR=0, save/load, shader and bridge checks are still required."
