# Isolated Pickle/Quickstarts runner. Does not modify the player's ModsConfig.xml.
[CmdletBinding()]
param(
    [string]$RimWorldDir = 'D:\SteamLibrary\steamapps\common\RimWorld',
    [ValidateRange(60, 1800)][int]$TimeoutSeconds = 600
)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$exe = Join-Path $RimWorldDir 'RimWorldWin64.exe'
$steamapps = (Resolve-Path (Join-Path $RimWorldDir '../..')).Path
$pickleDir = Join-Path $steamapps 'workshop/content/294100/3791648678'
$quickDir = Join-Path $steamapps 'workshop/content/294100/3793646067'
$testMod = Join-Path $RimWorldDir 'Mods/AncientMedievalJapanWaterworks.E2E'
$results = Join-Path $root 'TestResults/E2E'
$save = Join-Path $results 'SaveData'
$report = Join-Path $results 'Reports'
$log = Join-Path $report 'Player.log'
function Check([bool]$value, [string]$message) {
    if (-not $value) { throw "[ERROR] $message" }
}
Check (Test-Path -LiteralPath $exe) "RimWorld executable missing: $exe"
Check (Test-Path -LiteralPath $pickleDir) 'Pickle Workshop mod 3791648678 missing.'
Check (Test-Path -LiteralPath $quickDir) 'Quickstarts Workshop mod 3793646067 missing.'
$pickle = Get-ChildItem -LiteralPath $pickleDir -Filter 'RimWorks.Pickle.dll' -File -Recurse | Select-Object -First 1 -ExpandProperty FullName
$quick = Get-ChildItem -LiteralPath $quickDir -Filter 'Quickstarts.dll' -File -Recurse | Select-Object -First 1 -ExpandProperty FullName
Check (-not [string]::IsNullOrEmpty($pickle)) 'Pickle assembly missing.'
Check (-not [string]::IsNullOrEmpty($quick)) 'Quickstarts assembly missing.'
& (Join-Path $PSScriptRoot 'validate-source.ps1') -RimWorldDir $RimWorldDir
Check ($LASTEXITCODE -eq 0) 'Production build failed.'
$dotnet = Get-Command dotnet -ErrorAction Stop
& $dotnet.Source build (Join-Path $root 'Tests/E2E/Quickstart.csproj') -c Release "-p:RimWorldDir=$RimWorldDir" "-p:QuickstartsDll=$quick"
Check ($LASTEXITCODE -eq 0) 'E2E Quickstart build failed.'
& $dotnet.Source build (Join-Path $root 'Tests/E2E/Steps.csproj') -c Release "-p:RimWorldDir=$RimWorldDir" "-p:PickleDll=$pickle"
Check ($LASTEXITCODE -eq 0) 'E2E Pickle steps build failed.'

# Only delete the known E2E mod; refuse to erase a different existing folder.
if (Test-Path -LiteralPath $testMod) {
    Check ((Resolve-Path -LiteralPath $testMod).Path -eq (Join-Path $RimWorldDir 'Mods/AncientMedievalJapanWaterworks.E2E')) 'Unexpected test Mod path.'
    $about = Join-Path $testMod 'About/About.xml'
    Check ((Test-Path $about) -and
        ((Get-Content -LiteralPath $about -Raw) -match '<packageId>sucro.ancientmedievaljapan.waterworks.e2e</packageId>')) 'Refusing to replace an unrelated Mods folder.'
    Remove-Item -LiteralPath $testMod -Recurse -Force
}
foreach ($p in @($save, $report)) {
    if (Test-Path -LiteralPath $p) {
        Check ((Resolve-Path -LiteralPath $p).Path.StartsWith($results + [IO.Path]::DirectorySeparatorChar)) 'Unexpected test result path.'
        Move-Item -LiteralPath $p -Destination ($p + '-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
    }
}
foreach ($p in @(
    (Join-Path $testMod 'About'),
    (Join-Path $testMod 'Assemblies'),
    (Join-Path $testMod 'Pickle/Assemblies'),
    (Join-Path $testMod 'Pickle/Features'),
    (Join-Path $save 'Config'),
    $report
)) { $null = New-Item -ItemType Directory -Force -Path $p }
Copy-Item -LiteralPath (Join-Path $root 'Tests/E2E/TestMod/About/About.xml') -Destination (Join-Path $testMod 'About/About.xml')
Copy-Item -LiteralPath (Join-Path $root 'Tests/E2E/TestMod/Pickle/Features/waterworks-core.feature') -Destination (Join-Path $testMod 'Pickle/Features/waterworks-core.feature')
Copy-Item -LiteralPath (Join-Path $results 'Build/Quickstart/AncientMedievalJapanWaterworks.E2E.Quickstart.dll') -Destination (Join-Path $testMod 'Assemblies')
Copy-Item -LiteralPath (Join-Path $results 'Build/Steps/AncientMedievalJapanWaterworks.E2E.Steps.dll') -Destination (Join-Path $testMod 'Pickle/Assemblies')

$normal = Join-Path $env:USERPROFILE 'AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Config'
$normalConfig = Join-Path $normal 'ModsConfig.xml'
$normalPrefs = Join-Path $normal 'Prefs.xml'
Check (Test-Path -LiteralPath $normalConfig) 'Original ModsConfig.xml missing.'
Check (Test-Path -LiteralPath $normalPrefs) 'Original Prefs.xml missing.'
[xml]$settings = Get-Content -LiteralPath $normalConfig -Raw
[xml]$prefs = Get-Content -LiteralPath $normalPrefs -Raw
$dev = $prefs.SelectSingleNode('//devMode')
if ($null -eq $dev) {
    $dev = $prefs.CreateElement('devMode')
    $null = $prefs.DocumentElement.AppendChild($dev)
}
$dev.InnerText = 'True'
$prefs.Save((Join-Path $save 'Config/Prefs.xml'))
$version = [System.Security.SecurityElement]::Escape([string]$settings.ModsConfigData.version)
$isolated = @"
<?xml version="1.0" encoding="utf-8"?>
<ModsConfigData><version>$version</version>
  <activeMods>
    <li>brrainz.harmony</li>
    <li>ludeon.rimworld</li>
    <li>sucro.ancientmedievaljapan.waterworks</li>
    <li>rimworks.rimlogging</li>
    <li>rimworks.pickle</li>
    <li>rimworks.quickstarts</li>
    <li>sucro.ancientmedievaljapan.waterworks.e2e</li>
  </activeMods>
  <knownExpansions />
</ModsConfigData>
"@
[System.IO.File]::WriteAllText((Join-Path $save 'Config/ModsConfig.xml'),
    $isolated, (New-Object System.Text.UTF8Encoding($false)))
Write-Host '[INFO] Normal ModsConfig.xml was not modified.'
Write-Host "[INFO] Isolated SaveData: $save"
Write-Host '[INFO] Eight Pickle graph/width/wetland/bridge/pawn-work/save-load scenarios; render path remains enabled.'
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $exe
$psi.WorkingDirectory = $RimWorldDir
$psi.UseShellExecute = $false
$psi.CreateNoWindow = $true
$psi.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden
$psi.Arguments = @(
    '-savedatafolder="' + $save + '"',
    '-logFile "' + $log + '"',
    '-pickle-run="waterworks-core.feature"',
    '-pickle-mode=fast',
    '-pickle-report-dir="' + $report + '"',
    '-pickle-no-browser',
    '-pickle-run-timeout=8'
) -join ' '
$process = New-Object System.Diagnostics.Process
$process.StartInfo = $psi
try {
    Check ($process.Start()) 'Failed to start isolated RimWorld.'
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        try { $process.Kill(); $process.WaitForExit() } catch {}
        throw "Pickle exceeded $TimeoutSeconds seconds."
    }
    Check ($process.ExitCode -eq 0) "Pickle process exited $($process.ExitCode)."
}
finally { $process.Dispose() }
$summaryPath = Join-Path $report 'summary.json'
Check (Test-Path -LiteralPath $summaryPath) "Fresh Pickle summary missing: $summaryPath"
$summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
Check (([int]$summary.total -eq 8) -and ([int]$summary.passed -eq 8) -and
    ([int]$summary.failed -eq 0) -and ([int]$summary.skipped -eq 0)) "Pickle 7/7 gate failed: $summaryPath"
$required = @(
    'Waterworks production Defs load correctly',
    'Four-direction canal branches connect and disconnect',
    'Canal width stays one cell',
    'Standing freshwater requires nine adjacent cells',
    'Vanilla bridge preserves canal flow and terrain restoration',
    'Construction pawn completes real dig and fill jobs',
    'Save and reload restores canal supply and original ground'
)
$names = @($summary.scenarios | ForEach-Object { [string]$_.name })
foreach ($name in $required) { Check ($names -contains $name) "Missing scenario: $name" }
Check (Test-Path -LiteralPath $log) 'Isolated Player.log missing.'
$errorCount = @([regex]::Matches((Get-Content -LiteralPath $log -Raw), '(?im)^.*\[ERROR\].*$')).Count
Check ($errorCount -eq 0) "$errorCount ERROR-level runtime entries: $log"
Write-Host '[OK] Eight Waterworks Pickle scenarios passed; no [ERROR] runtime lines.'
Write-Host "[INFO] Reports: $report"
Write-Warning 'Visual rendering and addition to an existing save remain open.'
