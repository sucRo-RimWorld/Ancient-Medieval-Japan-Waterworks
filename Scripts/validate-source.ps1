# Non-interactive static and compile preflight. Does not launch RimWorld.
[CmdletBinding()]
param(
    [string]$RimWorldDir = $env:RIMWORLD_DIR
)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
# Parse every repository PowerShell script before building or starting RimWorld.
# This catches invalid string interpolation such as "$phase:" at source level.
$parseMessages = @()
foreach ($scriptFile in (Get-ChildItem -LiteralPath (Join-Path $root 'Scripts') -File -Filter '*.ps1')) {
    $tokens = $null
    $parseErrors = $null
    [void][System.Management.Automation.Language.Parser]::ParseFile(
        $scriptFile.FullName, [ref]$tokens, [ref]$parseErrors
    )
    foreach ($parseError in $parseErrors) {
        $parseMessages += "$($scriptFile.Name) line $($parseError.Extent.StartLineNumber): $($parseError.Message)"
    }
}
if ($parseMessages.Count -ne 0) {
    throw ("PowerShell parse preflight failed:`n" + ($parseMessages -join "`n"))
}

$python = Get-Command python -ErrorAction SilentlyContinue
if (-not $python) { throw "Python 3 is required for the Waterworks static audit." }
& $python.Source (Join-Path $root "Tests/static_audit.py")
if ($LASTEXITCODE -ne 0) { throw "Waterworks static audit failed (exit $LASTEXITCODE)." }

# Explicit -RimWorldDir overrides the environment. Otherwise, infer the
# game from the checkout's usual <RimWorld>/Mods/<mod> folder structure.
if ([string]::IsNullOrWhiteSpace($RimWorldDir)) {
    $modsFolder = Split-Path -Parent $root
    if ((Split-Path -Leaf $modsFolder) -ieq "Mods") {
        $candidate = Split-Path -Parent $modsFolder
        $candidateAssembly = Join-Path $candidate "RimWorldWin64_Data/Managed/Assembly-CSharp.dll"
        if (Test-Path -LiteralPath $candidateAssembly) {
            $RimWorldDir = $candidate
            Write-Output "[INFO] Auto-detected RimWorldDir: $RimWorldDir"
        }
    }
}
if ([string]::IsNullOrWhiteSpace($RimWorldDir)) {
    throw "RimWorld installation not found beside this Mod checkout. Specify -RimWorldDir or set RIMWORLD_DIR. C# build did not run."
}
$assembly = Join-Path $RimWorldDir "RimWorldWin64_Data/Managed/Assembly-CSharp.dll"
if (-not (Test-Path $assembly)) { throw "RimWorld Assembly-CSharp.dll not found: $assembly" }
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) { throw ".NET SDK and net472 targeting pack are required." }
& $dotnet.Source build (Join-Path $root "Source/AncientMedievalJapanWaterworks.csproj") -c Release "-p:RimWorldDir=$RimWorldDir"
if ($LASTEXITCODE -ne 0) { throw "Waterworks C# build failed (exit $LASTEXITCODE)." }

Write-Output "[OK] Waterworks static audit and C# build completed."
Write-Warning "Pickle/RimTest Redux, runtime ERROR=0, save/load, shader and bridge checks are still required."
