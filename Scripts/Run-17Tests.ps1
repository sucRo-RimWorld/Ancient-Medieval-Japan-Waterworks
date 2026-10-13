# RimWorld 1.7 unstable test-only runner. Production C# and 1.6 entry points are untouched.
[CmdletBinding()]
param(
    [string]$GameRoot = 'D:\SteamLibrary\steamapps\common\RimWorld',
    [ValidateRange(60, 1800)][int]$TimeoutSeconds = 600
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RimWorldRuntime.ps1')
$game = Get-RimWorldRuntime $GameRoot
if (-not $game.Modern) { throw 'Use Scripts/run-e2e.ps1 for RimWorld 1.6.' }
$repo = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$mods = Join-Path $game.Root 'Mods'
$source = Join-Path $repo 'About/About.xml'
[xml]$sourceAbout = Get-Content -LiteralPath $source -Raw
if ([string]$sourceAbout.ModMetaData.packageId -ne 'sucro.ancientmedievaljapan.waterworks') {
    throw 'Unexpected Waterworks source identity'
}
# Do not stage a duplicate production Mod or modify installed source metadata.
if ([IO.Path]::GetFullPath((Split-Path -Parent $repo)) -ne [IO.Path]::GetFullPath($mods)) {
    throw '1.7 test requires the actual Waterworks checkout directly under RimWorld/Mods.'
}
$production = Join-Path $repo 'Assemblies/AncientMedievalJapanWaterworks.dll'
if (-not (Test-Path -LiteralPath $production -PathType Leaf)) {
    throw 'Build and inspect the existing Waterworks production DLL separately before 1.7 testing.'
}
$sourceHash = (Get-FileHash -LiteralPath $production -Algorithm SHA256).Hash
$normal = Join-Path $env:USERPROFILE 'AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Config'
$normalHashes = @{}
foreach ($name in @('ModsConfig.xml', 'Prefs.xml')) {
    $normalHashes[$name] = (Get-FileHash -LiteralPath (Join-Path $normal $name) -Algorithm SHA256).Hash
}
$workshop = [IO.Path]::GetFullPath((Join-Path $game.Root '../../workshop/content/294100'))
$quick = Find-RimWorldModAssembly (Join-Path $workshop '3793646067') 'Quickstarts.dll' '1.7'
$pickle = Find-RimWorldModAssembly (Join-Path $workshop '3791648678') 'RimWorks.Pickle.dll' '1.7'
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$result = Join-Path $repo ('TestResults/RimWorld17-' + $timestamp)
$fixture = Join-Path $mods 'AncientMedievalJapanWaterworks.E2E17'
if (Test-Path -LiteralPath $fixture) { throw 'Existing 1.7 test fixture requires inspection; refusing overwrite.' }
$marker = Join-Path $fixture '.AMJW-E2E17-OWNED'
$owned = $false
New-Item -ItemType Directory -Force -Path $result | Out-Null
try {
    New-Item -ItemType Directory -Path $fixture | Out-Null
    $owned = $true
    Set-Content -LiteralPath $marker -Value 'sucro.ancientmedievaljapan.waterworks.e2e17' -Encoding Ascii
    foreach ($relative in @('About', 'Assemblies', 'Pickle/Assemblies', 'Pickle/Features')) {
        New-Item -ItemType Directory -Force -Path (Join-Path $fixture $relative) | Out-Null
    }
    [xml]$about = Get-Content -LiteralPath (Join-Path $repo 'Tests/E2E/TestMod/About/About.xml') -Raw
    $about.ModMetaData.packageId = 'sucro.ancientmedievaljapan.waterworks.e2e17'
    $versionNode = $about.SelectSingleNode('/ModMetaData/supportedVersions')
    if ($null -eq $versionNode) { throw 'Test-only About lacks supportedVersions.' }
    if (@($versionNode.li | ForEach-Object { [string]$_ }) -notcontains '1.7') {
        $li = $about.CreateElement('li')
        $li.InnerText = '1.7'
        [void]$versionNode.AppendChild($li)
    }
    $about.Save((Join-Path $fixture 'About/About.xml'))
    Copy-Item -LiteralPath (Join-Path $repo 'Tests/E2E/TestMod/Pickle/Features/waterworks-core.feature') -Destination (Join-Path $fixture 'Pickle/Features')
    function Get-TestSources([string]$project) {
        [xml]$xml = Get-Content -LiteralPath $project -Raw
        $directory = Split-Path -Parent $project
        @($xml.SelectNodes('//Compile') | ForEach-Object {
            $path = Join-Path $directory ([string]$_.Include)
            if (-not (Test-Path -LiteralPath $path)) { throw "Missing test C# source: $path" }
            $path
        })
    }
    Invoke-RimWorldTestCompile $game.Root (Join-Path $fixture 'Assemblies/AncientMedievalJapanWaterworks.E2E.Quickstart.dll') `
        (Get-TestSources (Join-Path $repo 'Tests/E2E/Quickstart.csproj')) @($quick)
    Invoke-RimWorldTestCompile $game.Root (Join-Path $fixture 'Pickle/Assemblies/AncientMedievalJapanWaterworks.E2E.Steps.dll') `
        (Get-TestSources (Join-Path $repo 'Tests/E2E/Steps.csproj')) @($pickle, $production)

    $save = Join-Path $result 'SaveData'
    $configDir = Join-Path $save 'Config'
    New-Item -ItemType Directory -Force -Path $configDir | Out-Null
    [xml]$config = Get-Content -LiteralPath (Join-Path $normal 'ModsConfig.xml') -Raw
    $active = $config.SelectSingleNode('/ModsConfigData/activeMods')
    if ($null -eq $active) { throw 'Normal ModsConfig lacks activeMods.' }
    $active.RemoveAll()
    foreach ($id in @('brrainz.harmony','ludeon.rimworld','sucro.ancientmedievaljapan.waterworks',
        'rimworks.rimlogging','rimworks.pickle','rimworks.quickstarts',
        'sucro.ancientmedievaljapan.waterworks.e2e17')) {
        $li = $config.CreateElement('li')
        $li.InnerText = $id
        [void]$active.AppendChild($li)
    }
    $config.ModsConfigData.version = $game.Version.Split(' ')[0]
    $config.Save((Join-Path $configDir 'ModsConfig.xml'))
    [xml]$prefs = Get-Content -LiteralPath (Join-Path $normal 'Prefs.xml') -Raw
    foreach ($pair in @(@('devMode','True'), @('resetModsConfigOnCrash','False'),
        @('fullscreen','False'), @('runInBackground','True'))) {
        $node = $prefs.DocumentElement.SelectSingleNode($pair[0])
        if ($null -eq $node) {
            $node = $prefs.CreateElement($pair[0])
            [void]$prefs.DocumentElement.AppendChild($node)
        }
        $node.InnerText = $pair[1]
    }
    $prefs.Save((Join-Path $configDir 'Prefs.xml'))
    $report = Join-Path $result 'Reports'
    $arguments = '-savedatafolder="' + $save + '" -pickle-run="waterworks-core.feature" ' +
        '-pickle-mode=fast -pickle-report-dir="' + $report +
        '" -pickle-no-browser -pickle-no-http -pickle-run-timeout=8'
    Invoke-RimWorldIsolated $game.Root $arguments (Join-Path $result 'Player.log') $TimeoutSeconds
    $summaryPath = Join-Path $report 'summary.json'
    if (-not (Test-Path -LiteralPath $summaryPath)) { throw 'Fresh Waterworks 1.7 Pickle summary missing.' }
    $summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
    $required = @(
        'Waterworks production Defs load correctly',
        'Four-direction canal branches connect and disconnect',
        'Canal width stays one cell',
        'Standing freshwater requires nine adjacent cells',
        'Mud and Marsh allow excavation without becoming freshwater sources',
        'Vanilla bridge preserves canal flow and terrain restoration',
        'Construction pawn completes real dig and fill jobs',
        'Save and reload restores canal supply and original ground'
    )
    $actual = @($summary.scenarios | ForEach-Object { [string]$_.name })
    if ($summary.total -ne 8 -or $summary.passed -ne 8 -or $summary.failed -ne 0 -or
        $summary.skipped -ne 0 -or (@($actual | Select-Object -Unique)).Count -ne 8) {
        throw "Waterworks 1.7 requires exactly 8 clean Pickle scenarios: $summaryPath"
    }
    foreach ($name in $required) {
        if ($actual -notcontains $name) { throw "Missing Waterworks 1.7 scenario: $name" }
    }
    Write-Output "[OK] Waterworks $($game.Version) Pickle 8/8; native ERROR 0. Evidence: $result"
}
finally {
    if ($owned) {
        if ((Test-Path -LiteralPath $marker) -and
            ((Get-Content -LiteralPath $marker -Raw).Trim() -eq 'sucro.ancientmedievaljapan.waterworks.e2e17')) {
            Remove-Item -LiteralPath $fixture -Recurse -Force
        } else { throw 'Test fixture marker missing; refusing unsafe cleanup.' }
    }
    if ((Get-FileHash -LiteralPath $production -Algorithm SHA256).Hash -ne $sourceHash) {
        throw 'Waterworks production DLL changed during the test.'
    }
    foreach ($name in $normalHashes.Keys) {
        if ((Get-FileHash -LiteralPath (Join-Path $normal $name) -Algorithm SHA256).Hash -ne $normalHashes[$name]) {
            throw "Normal RimWorld $name changed during the test."
        }
    }
}
