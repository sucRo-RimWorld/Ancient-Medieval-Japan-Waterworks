# Separate rendered-frame evidence runner; normal ModsConfig and saves are untouched.
[CmdletBinding()]
param([string]$RimWorldDir = 'D:\SteamLibrary\steamapps\common\RimWorld',
      [ValidateRange(180,1800)][int]$TimeoutSeconds = 600)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$game = Join-Path $RimWorldDir 'RimWorldWin64.exe'
$steamapps = (Resolve-Path (Join-Path $RimWorldDir '../..')).Path
$pickleRoot = Join-Path $steamapps 'workshop/content/294100/3791648678'
$quickRoot = Join-Path $steamapps 'workshop/content/294100/3793646067'
$testMod = Join-Path $RimWorldDir 'Mods/AncientMedievalJapanWaterworks.VisualE2E'
$results = Join-Path $root 'TestResults/Visual'
$save = Join-Path $results 'SaveData'
$report = Join-Path $results 'Reports'
$log = Join-Path $report 'Player.log'
function Require([bool]$ok,[string]$message) { if (-not $ok) { throw "[ERROR] $message" } }
foreach ($item in @($game,$pickleRoot,$quickRoot)) { Require (Test-Path -LiteralPath $item) "Missing: $item" }
$pickle = Get-ChildItem $pickleRoot -Recurse -File -Filter RimWorks.Pickle.dll | Select-Object -First 1 -ExpandProperty FullName
$quick = Get-ChildItem $quickRoot -Recurse -File -Filter Quickstarts.dll | Select-Object -First 1 -ExpandProperty FullName
Require ([bool]$pickle -and [bool]$quick) 'Pickle/Quickstarts DLL missing'
& (Join-Path $PSScriptRoot 'validate-source.ps1') -RimWorldDir $RimWorldDir
Require ($LASTEXITCODE -eq 0) 'Source validation failed'
$dotnet = (Get-Command dotnet -ErrorAction Stop).Source
& $dotnet build (Join-Path $root 'Tests/E2E/Quickstart.csproj') -c Release "-p:RimWorldDir=$RimWorldDir" "-p:QuickstartsDll=$quick"
Require ($LASTEXITCODE -eq 0) 'Quickstart build failed'
& $dotnet build (Join-Path $root 'Tests/E2E/Steps.csproj') -c Release "-p:RimWorldDir=$RimWorldDir" "-p:PickleDll=$pickle"
Require ($LASTEXITCODE -eq 0) 'Steps build failed'
if (Test-Path $testMod) {
  $about = Join-Path $testMod 'About/About.xml'
  Require ((Test-Path $about) -and ((Get-Content $about -Raw) -match '<packageId>sucro.ancientmedievaljapan.waterworks.visuale2e</packageId>')) 'Refusing to remove unrelated mod'
  Remove-Item $testMod -Recurse -Force
}
if (Test-Path $results) { Remove-Item $results -Recurse -Force }
foreach ($dir in @('About','Assemblies','Defs/ThingCategoryDefs','Pickle/Assemblies','Pickle/Features')) {
  $null = New-Item -ItemType Directory -Force -Path (Join-Path $testMod $dir)
}
$null = New-Item -ItemType Directory -Force -Path (Join-Path $save 'Config')
$null = New-Item -ItemType Directory -Force -Path $report
Copy-Item (Join-Path $root 'Tests/E2E/VisualMod/About/About.xml') (Join-Path $testMod 'About/About.xml')
Copy-Item (Join-Path $root 'Tests/E2E/VisualMod/Defs/ThingCategoryDefs/AMJW_VisualMarker.xml') (Join-Path $testMod 'Defs/ThingCategoryDefs')
Copy-Item (Join-Path $root 'Tests/E2E/TestMod/Pickle/Features/waterworks-visual.feature') (Join-Path $testMod 'Pickle/Features')
Copy-Item (Join-Path $root 'TestResults/E2E/Build/Quickstart/AncientMedievalJapanWaterworks.E2E.Quickstart.dll') (Join-Path $testMod 'Assemblies')
Copy-Item (Join-Path $root 'TestResults/E2E/Build/Steps/AncientMedievalJapanWaterworks.E2E.Steps.dll') (Join-Path $testMod 'Pickle/Assemblies')
$normal = Join-Path $env:USERPROFILE 'AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Config'
[xml]$config = Get-Content (Join-Path $normal 'ModsConfig.xml') -Raw
[xml]$prefs = Get-Content (Join-Path $normal 'Prefs.xml') -Raw
$dev = $prefs.SelectSingleNode('//devMode')
if ($null -eq $dev) { $dev=$prefs.CreateElement('devMode'); $null=$prefs.DocumentElement.AppendChild($dev) }
$dev.InnerText = 'True'
$prefs.Save((Join-Path $save 'Config/Prefs.xml'))
$version = [System.Security.SecurityElement]::Escape([string]$config.ModsConfigData.version)
$xml = @"
<?xml version="1.0" encoding="utf-8"?>
<ModsConfigData><version>$version</version><activeMods>
<li>brrainz.harmony</li><li>ludeon.rimworld</li>
<li>sucro.ancientmedievaljapan.waterworks</li>
<li>rimworks.rimlogging</li><li>rimworks.pickle</li><li>rimworks.quickstarts</li>
<li>sucro.ancientmedievaljapan.waterworks.visuale2e</li>
</activeMods><knownExpansions /></ModsConfigData>
"@
[System.IO.File]::WriteAllText((Join-Path $save 'Config/ModsConfig.xml'),$xml,(New-Object System.Text.UTF8Encoding($false)))
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName=$game
$psi.WorkingDirectory=$RimWorldDir
$psi.UseShellExecute=$false
$psi.CreateNoWindow=$true
$psi.WindowStyle=[System.Diagnostics.ProcessWindowStyle]::Hidden
$psi.Arguments= @('-savedatafolder="' + $save + '"','-logFile "' + $log + '"','-pickle-run="waterworks-visual.feature"','-pickle-mode=fast','-pickle-report-dir="' + $report + '"','-pickle-no-browser','-pickle-run-timeout=8') -join ' '
$process=New-Object System.Diagnostics.Process
$process.StartInfo=$psi
try {
 Require ($process.Start()) 'Could not start RimWorld'
 if (-not $process.WaitForExit($TimeoutSeconds*1000)) {
   try {$process.Kill(); $process.WaitForExit()} catch {}
   throw 'Visual E2E timed out'
 }
 Require ($process.ExitCode -eq 0) "RimWorld exited $($process.ExitCode)"
} finally { $process.Dispose() }
$summaryPath=Join-Path $report 'summary.json'
Require (Test-Path $summaryPath) 'No Pickle summary'
$summary=Get-Content $summaryPath -Raw | ConvertFrom-Json
Require (($summary.total -eq 1) -and ($summary.passed -eq 1) -and ($summary.failed -eq 0) -and ($summary.skipped -eq 0)) 'Visual Pickle 1/1 did not pass'
Require (Test-Path $log) 'No Player.log'
$errors=@(Select-String -LiteralPath $log -Pattern '\[ERROR\]' -Context 0,12)
if ($errors.Count -gt 0) {
 foreach ($e in $errors) { Write-Host $e.Line; $e.Context.PostContext | ForEach-Object {Write-Host $_} }
 throw "[ERROR] Visual run produced $($errors.Count) runtime ERRORs"
}
$captures=Join-Path $save 'WaterworksVisual'
foreach ($name in @('connected.png','disconnected.png','restored.png')) {
 $path=Join-Path $captures $name
 Require (Test-Path $path) "Screenshot missing: $path"
 $bytes=[System.IO.File]::ReadAllBytes($path)
 Require ($bytes.Length -gt 24 -and $bytes[0] -eq 137 -and $bytes[1] -eq 80 -and $bytes[2] -eq 78 -and $bytes[3] -eq 71) "Invalid PNG: $path"
}
Require (Test-Path (Join-Path $captures 'manifest.txt')) 'Visual manifest missing'
Write-Host "[OK] Visual Pickle 1/1, runtime ERROR=0, three PNG files saved: $captures"
Write-Warning 'PNG collection does not establish visual acceptance; images require review.'
