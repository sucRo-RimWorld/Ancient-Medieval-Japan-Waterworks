# A: Vanilla-only scratch save, B: cold-start Waterworks on that saved game.
# Normal ModsConfig and user saves remain untouched.
[CmdletBinding()]
param(
    [string]$RimWorldDir = 'D:\SteamLibrary\steamapps\common\RimWorld',
    [ValidateRange(180,1800)][int]$TimeoutSeconds = 600
)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$gameExe = Join-Path $RimWorldDir 'RimWorldWin64.exe'
$steamApps = (Resolve-Path (Join-Path $RimWorldDir '../..')).Path
$pickleRoot = Join-Path $steamApps 'workshop/content/294100/3791648678'
$quickRoot = Join-Path $steamApps 'workshop/content/294100/3793646067'
$bootstrapMod = Join-Path $RimWorldDir 'Mods/AncientMedievalJapanWaterworks.VanillaBootstrap'
$addMod = Join-Path $RimWorldDir 'Mods/AncientMedievalJapanWaterworks.AddToSaveE2E'
$results = Join-Path $root 'TestResults/AddToSave'
$saveData = Join-Path $results 'SaveData'
$baselineSave = Join-Path $saveData 'Saves/waterworks-before-install.rws'
$normalConfig = Join-Path $env:USERPROFILE 'AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Config/ModsConfig.xml'
$normalPrefs = Join-Path $env:USERPROFILE 'AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Config/Prefs.xml'

function Require([bool]$condition, [string]$message) {
    if (-not $condition) { throw "[ERROR] $message" }
}
foreach ($p in @($gameExe, $pickleRoot, $quickRoot, $normalConfig, $normalPrefs)) {
    Require (Test-Path -LiteralPath $p) "Missing required input: $p"
}
$pickleDll = Get-ChildItem -LiteralPath $pickleRoot -Recurse -File -Filter 'RimWorks.Pickle.dll' | Select-Object -First 1 -ExpandProperty FullName
$quickDll = Get-ChildItem -LiteralPath $quickRoot -Recurse -File -Filter 'Quickstarts.dll' | Select-Object -First 1 -ExpandProperty FullName
Require (-not [string]::IsNullOrEmpty($pickleDll)) 'Pickle DLL not found.'
Require (-not [string]::IsNullOrEmpty($quickDll)) 'Quickstarts DLL not found.'

# Build everything before creating or modifying the isolated run profiles.
& (Join-Path $PSScriptRoot 'validate-source.ps1') -RimWorldDir $RimWorldDir
Require ($LASTEXITCODE -eq 0) 'Waterworks production DLL build failed.'
$dotnet = (Get-Command dotnet -ErrorAction Stop).Source
& $dotnet build (Join-Path $root 'Tests/E2E/Quickstart.csproj') -c Release "-p:RimWorldDir=$RimWorldDir" "-p:QuickstartsDll=$quickDll"
Require ($LASTEXITCODE -eq 0) 'Vanilla Quickstart DLL failed to build.'
& $dotnet build (Join-Path $root 'Tests/E2E/BootstrapSteps.csproj') -c Release "-p:RimWorldDir=$RimWorldDir" "-p:PickleDll=$pickleDll"
Require ($LASTEXITCODE -eq 0) 'Vanilla Pickle bootstrap DLL failed to build.'
& $dotnet build (Join-Path $root 'Tests/E2E/Steps.csproj') -c Release "-p:RimWorldDir=$RimWorldDir" "-p:PickleDll=$pickleDll"
Require ($LASTEXITCODE -eq 0) 'Waterworks Pickle steps DLL failed to build.'

function Stage-TestMod([string]$destination, [string]$expectedId, [string]$sourceName) {
    if (Test-Path -LiteralPath $destination) {
        $aboutFile = Join-Path $destination 'About/About.xml'
        Require ((Test-Path -LiteralPath $aboutFile) -and ((Get-Content -LiteralPath $aboutFile -Raw) -match [regex]::Escape("<packageId>$expectedId</packageId>"))) "Refusing to replace unrelated Mod directory: $destination"
        Remove-Item -LiteralPath $destination -Recurse -Force
    }
    foreach ($relative in @('About','Assemblies','Pickle/Assemblies','Pickle/Features')) {
        $null = New-Item -ItemType Directory -Force -Path (Join-Path $destination $relative)
    }
    Copy-Item -LiteralPath (Join-Path $root "Tests/E2E/$sourceName/About/About.xml") -Destination (Join-Path $destination 'About/About.xml')
}
Stage-TestMod $bootstrapMod 'sucro.ancientmedievaljapan.waterworks.bootstrap' 'BootstrapMod'
Stage-TestMod $addMod 'sucro.ancientmedievaljapan.waterworks.addtosave' 'AddToSaveMod'
Copy-Item -LiteralPath (Join-Path $root 'TestResults/E2E/Build/Quickstart/AncientMedievalJapanWaterworks.E2E.Quickstart.dll') -Destination (Join-Path $bootstrapMod 'Assemblies')
Copy-Item -LiteralPath (Join-Path $root 'TestResults/E2E/Build/Bootstrap/AncientMedievalJapanWaterworks.E2E.Bootstrap.dll') -Destination (Join-Path $bootstrapMod 'Pickle/Assemblies')
Copy-Item -LiteralPath (Join-Path $root 'TestResults/E2E/Build/Steps/AncientMedievalJapanWaterworks.E2E.Steps.dll') -Destination (Join-Path $addMod 'Pickle/Assemblies')
Copy-Item -LiteralPath (Join-Path $root 'Tests/E2E/BootstrapMod/Pickle/Features/waterworks-before-install.feature') -Destination (Join-Path $bootstrapMod 'Pickle/Features')
Copy-Item -LiteralPath (Join-Path $root 'Tests/E2E/AddToSaveMod/Pickle/Features/waterworks-add-to-save.feature') -Destination (Join-Path $addMod 'Pickle/Features')

# Only the dedicated scratch folder is reset. Never touch a user's game saves.
if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Recurse -Force }
$null = New-Item -ItemType Directory -Force -Path (Join-Path $saveData 'Config')
[xml]$config = Get-Content -LiteralPath $normalConfig -Raw
[xml]$prefs = Get-Content -LiteralPath $normalPrefs -Raw
$dev = $prefs.SelectSingleNode('//devMode')
if ($null -eq $dev) {
    $dev = $prefs.CreateElement('devMode')
    $null = $prefs.DocumentElement.AppendChild($dev)
}
$dev.InnerText = 'True'
$prefs.Save((Join-Path $saveData 'Config/Prefs.xml'))
$version = [System.Security.SecurityElement]::Escape([string]$config.ModsConfigData.version)

function Write-IsolatedConfig([bool]$withWaterworks) {
    $common = @('brrainz.harmony','ludeon.rimworld','rimworks.rimlogging','rimworks.pickle','rimworks.quickstarts','sucro.ancientmedievaljapan.waterworks.bootstrap')
    $extra = @()
    if ($withWaterworks) {
        $extra = @('sucro.ancientmedievaljapan.waterworks','sucro.ancientmedievaljapan.waterworks.addtosave')
    }
    $all = $common + $extra
    $items = ($all | ForEach-Object { '    <li>' + $_ + '</li>' }) -join [Environment]::NewLine
    $xml = @"
<?xml version="1.0" encoding="utf-8"?>
<ModsConfigData>
  <version>$version</version>
  <activeMods>
$items
  </activeMods>
  <knownExpansions />
</ModsConfigData>
"@
    [System.IO.File]::WriteAllText((Join-Path $saveData 'Config/ModsConfig.xml'), $xml, (New-Object System.Text.UTF8Encoding($false)))
    if (-not $withWaterworks) {
        Require (-not $xml.Contains('<li>sucro.ancientmedievaljapan.waterworks</li>')) 'Waterworks accidentally included in Vanilla baseline.'
    }
}

function Run-Phase([string]$feature, [string]$expectedScenario, [string]$phase) {
    $report = Join-Path $results "Reports/$phase"
    $null = New-Item -ItemType Directory -Force -Path $report
    $log = Join-Path $report 'Player.log'
    $info = New-Object System.Diagnostics.ProcessStartInfo
    $info.FileName = $gameExe
    $info.WorkingDirectory = $RimWorldDir
    $info.UseShellExecute = $false
    $info.CreateNoWindow = $true
    $info.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden
    $info.Arguments = @(
        '-savedatafolder="' + $saveData + '"',
        '-logFile "' + $log + '"',
        '-pickle-run="' + $feature + '"',
        '-pickle-mode=fast',
        '-pickle-report-dir="' + $report + '"',
        '-pickle-no-browser',
        '-pickle-run-timeout=8'
    ) -join ' '
    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $info
    try {
        Require ($process.Start()) "RimWorld did not start phase $phase."
        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
            try { $process.Kill(); $process.WaitForExit() } catch {}
            throw "RimWorld phase $phase exceeded $TimeoutSeconds seconds."
        }
        Require ($process.ExitCode -eq 0) "$phase exited with code $($process.ExitCode)."
    } finally { $process.Dispose() }
    $summaryFile = Join-Path $report 'summary.json'
    Require (Test-Path -LiteralPath $summaryFile) "$phase Pickle summary missing."
    $summary = Get-Content -LiteralPath $summaryFile -Raw | ConvertFrom-Json
    Require (([int]$summary.total -eq 1) -and ([int]$summary.passed -eq 1) -and ([int]$summary.failed -eq 0) -and ([int]$summary.skipped -eq 0)) "$phase requires a 1/1 Pickle pass: $summaryFile"
    $names = @($summary.scenarios | ForEach-Object { [string]$_.name })
    Require ($names -contains $expectedScenario) "Expected scenario missing: $expectedScenario"
    Require (Test-Path -LiteralPath $log) "Player.log missing for $phase."
    $errors = @([regex]::Matches((Get-Content -LiteralPath $log -Raw), '(?im)^.*\[ERROR\].*$')).Count
    Require ($errors -eq 0) "$phase produced $errors runtime ERROR lines: $log"
    Write-Host "[OK] ${phase}: 1/1 Pickle scenario, zero runtime ERROR."
}

Write-Host '[INFO] Phase A: make a Vanilla saved game WITHOUT Waterworks.'
Write-IsolatedConfig $false
Run-Phase 'waterworks-before-install.feature' 'Vanilla saved game is created without Waterworks' 'BeforeInstall'
Require (Test-Path -LiteralPath $baselineSave) "Vanilla baseline .rws not written: $baselineSave"

Write-Host '[INFO] Phase B: cold-start WITH Waterworks, load the Vanilla baseline.'
Write-IsolatedConfig $true
Run-Phase 'waterworks-add-to-save.feature' 'A Vanilla-only saved map safely accepts newly installed Waterworks' 'AfterInstall'
Write-Host '[OK] Existing-save add-install plus a second save round trip passed.'
Write-Host "[INFO] Reports: $(Join-Path $results 'Reports')"
Write-Warning 'Visual presentation and normal user-modpack compatibility remain independent gates.'
