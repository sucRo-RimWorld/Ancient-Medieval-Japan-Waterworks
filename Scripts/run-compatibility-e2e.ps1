# Isolated Pickle/Quickstarts runner. Does not modify the player's ModsConfig.xml.
[CmdletBinding()]
param(
    [string]$RimWorldDir = 'D:\SteamLibrary\steamapps\common\RimWorld',
    [ValidateSet('Solo','DBHOnly','Medieval')][string]$Profile = 'Medieval',
    [ValidateSet('Core','Integration')][string]$Suite = 'Integration',
    [switch]$NeedsDisabled,
    [switch]$LiteMode,
    [ValidateRange(60, 1800)][int]$TimeoutSeconds = 600
)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$exe = Join-Path $RimWorldDir 'RimWorldWin64.exe'
$steamapps = (Resolve-Path (Join-Path $RimWorldDir '../..')).Path
$pickleDir = Join-Path $steamapps 'workshop/content/294100/3791648678'
$quickDir = Join-Path $steamapps 'workshop/content/294100/3793646067'
$testMod = Join-Path $RimWorldDir 'Mods/AncientMedievalJapanWaterworks.E2E'
$results = Join-Path $root ('TestResults/Compatibility-' + $Profile + '-' + $Suite + '-' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
if ($LiteMode -and $Suite -ne 'Core') { throw 'LiteMode removes partner sluices; use Core suite' }
$feature = if ($Suite -eq 'Core') {'waterworks-core.feature'} else {'waterworks-medieval.feature'}
if ($Profile -ne 'Medieval' -and $Suite -eq 'Integration') { throw 'Integration requires Medieval profile' }
. (Join-Path $PSScriptRoot 'RimWorldRuntime.ps1')
if ((Get-RimWorldRuntime $RimWorldDir).Modern) { throw 'Use Run-17Tests.ps1 for RimWorld 1.7' }
$save = Join-Path $results 'SaveData'
$report = Join-Path $results 'Reports'
$log = Join-Path $report 'Player.log'
function Check([bool]$value, [string]$message) {
    if (-not $value) { throw "[ERROR] $message" }
}
Check (Test-Path -LiteralPath $exe) "RimWorld executable missing: $exe"
Check (Test-Path -LiteralPath $pickleDir) 'Pickle Workshop mod 3791648678 missing.'
Check (Test-Path -LiteralPath $quickDir) 'Quickstarts Workshop mod 3793646067 missing.'
$pickle = Find-RimWorldModAssembly $pickleDir 'RimWorks.Pickle.dll' '1.6'
$quick = Find-RimWorldModAssembly $quickDir 'Quickstarts.dll' '1.6'
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
        ((Get-Content -LiteralPath $about -Raw -Encoding UTF8) -match '<packageId>sucro.ancientmedievaljapan.waterworks.e2e</packageId>')) 'Refusing to replace an unrelated Mods folder.'
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
Copy-Item -LiteralPath (Join-Path $root ('Tests/E2E/TestMod/Pickle/Features/' + $feature)) -Destination (Join-Path $testMod ('Pickle/Features/' + $feature))
Copy-Item -LiteralPath (Join-Path $root 'TestResults/E2E/Build/Quickstart/AncientMedievalJapanWaterworks.E2E.Quickstart.dll') -Destination (Join-Path $testMod 'Assemblies')
Copy-Item -LiteralPath (Join-Path $root 'TestResults/E2E/Build/Steps/AncientMedievalJapanWaterworks.E2E.Steps.dll') -Destination (Join-Path $testMod 'Pickle/Assemblies')

$normal = Join-Path $env:USERPROFILE 'AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Config'
$normalConfig = Join-Path $normal 'ModsConfig.xml'
$normalPrefs = Join-Path $normal 'Prefs.xml'
$preserved = @{}
foreach ($p in @($normalConfig,$normalPrefs)) { $preserved[$p]=(Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash }
Check (Test-Path -LiteralPath $normalConfig) 'Original ModsConfig.xml missing.'
Check (Test-Path -LiteralPath $normalPrefs) 'Original Prefs.xml missing.'
[xml]$settings = Get-Content -LiteralPath $normalConfig -Raw -Encoding UTF8
[xml]$prefs = Get-Content -LiteralPath $normalPrefs -Raw -Encoding UTF8
$dev = $prefs.SelectSingleNode('//devMode')
if ($null -eq $dev) {
    $dev = $prefs.CreateElement('devMode')
    $null = $prefs.DocumentElement.AppendChild($dev)
}
$dev.InnerText = 'True'
$prefs.Save((Join-Path $save 'Config/Prefs.xml'))
$version = [System.Security.SecurityElement]::Escape([string]$settings.ModsConfigData.version)
$partnerIds = if ($Profile -eq 'Solo') {''} else {'<li>dubwise.dubsbadhygiene</li>'}
if ($Profile -eq 'Medieval') { $partnerIds += '<li>eldersign.dbhformedieval</li>' }
$isolated = @"
<?xml version="1.0" encoding="utf-8"?>
<ModsConfigData><version>$version</version>
  <activeMods>
    <li>brrainz.harmony</li>
    <li>ludeon.rimworld</li>
    $partnerIds
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
Write-Host "[INFO] $Profile $Suite; rendering remains enabled."
if ($NeedsDisabled -or $LiteMode) {
    $lite = if ($LiteMode) {'True'} else {'False'}
    [IO.File]::WriteAllText((Join-Path $save 'Config/Mod_836308268_DubsBadHygieneMod.xml'), '<SettingsBlock><ModSettings Class="DubsBadHygiene.Settings"><EnableBladder>False</EnableBladder><EnableHygiene>False</EnableHygiene><ThirstNeed>False</ThirstNeed><LiteMode>' + $lite + '</LiteMode></ModSettings></SettingsBlock>')
}
if ($LiteMode) { $env:AMJW_EXPECT_LITE='1' }
    if ($NeedsDisabled -and -not $LiteMode) { $env:AMJW_EXPECT_NO_NEEDS='1' }
$arguments = '-logFile "' + $log + '" -savedatafolder="' + $save + '" -pickle-run="' + $feature + '" ' +
    '-pickle-mode=fast -pickle-report-dir="' + $report +
    '" -pickle-no-browser -pickle-no-http -pickle-run-timeout=8'
try { Invoke-RimWorldIsolated $RimWorldDir $arguments $log $TimeoutSeconds }
finally {
    Remove-Item Env:AMJW_EXPECT_NO_NEEDS -ErrorAction SilentlyContinue
    Remove-Item Env:AMJW_EXPECT_LITE -ErrorAction SilentlyContinue
    foreach ($p in $preserved.Keys) {
        Check ((Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash -eq $preserved[$p]) "Normal profile changed: $p"
    }
}
$summaryPath = Join-Path $report 'summary.json'
Check (Test-Path -LiteralPath $summaryPath) "Fresh Pickle summary missing: $summaryPath"
$summary = Get-Content -LiteralPath $summaryPath -Raw -Encoding UTF8 | ConvertFrom-Json
$required = if ($Suite -eq 'Integration') {
    @('Separate maps never share supply and native map removal clears the provider',
          'Pawn opens native sluice and game ticks irrigate soil',
          'Natural source feeds native sluice once and disconnects safely',
      'Diagonal contact and disconnected Medieval chains never irrigate',
      'Native storage is charged first and survives natural source loss',
      'Real save reload preserves optional irrigation topology')
} else {
    @('Waterworks production Defs load correctly',
      'Four-direction canal branches connect and disconnect',
      'Canal width stays one cell',
      'Standing freshwater requires nine adjacent cells',
      'Mud and Marsh allow excavation without becoming freshwater sources',
      'Vanilla bridge preserves canal flow and terrain restoration',
      'Construction pawn completes real dig and fill jobs',
      'Save and reload restores canal supply and original ground')
}
Check ($summary.total -eq $required.Count -and $summary.passed -eq $required.Count -and
    $summary.failed -eq 0 -and $summary.skipped -eq 0) "Incomplete compatibility suite: $summaryPath"
$names = @($summary.scenarios | ForEach-Object { [string]$_.name })
foreach ($name in $required) { Check ($names -contains $name) "Missing scenario: $name" }
Check (Test-Path -LiteralPath $log) 'Isolated Player.log missing.'
$errorCount = @([regex]::Matches((Get-Content -LiteralPath $log -Raw -Encoding UTF8), '(?im)^.*\[ERROR\].*$')).Count
Check ($errorCount -eq 0) "$errorCount ERROR-level runtime entries: $log"
Write-Host "[OK] $Profile $Suite $($required.Count)/$($required.Count); runtime ERROR 0."
Write-Host "[INFO] Reports: $report"
Write-Warning 'Visual rendering and addition to an existing save remain open.'
