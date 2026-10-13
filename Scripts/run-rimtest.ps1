[CmdletBinding()]
param(
    [string]$RimWorldDir='D:\SteamLibrary\steamapps\common\RimWorld',
    [ValidateSet('Solo','DBHOnly','Medieval')][string]$Profile='Solo'
)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RimWorldRuntime.ps1')
$runtime=Get-RimWorldRuntime $RimWorldDir
$root=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$workshop=[IO.Path]::GetFullPath((Join-Path $RimWorldDir '../../workshop/content/294100'))
$mods=Join-Path $RimWorldDir 'Mods'
$fixture=Join-Path $mods 'AncientMedievalJapanWaterworks.RimTests'
if(Test-Path -LiteralPath $fixture){throw 'Stale RimTest fixture: inspect before retrying.'}
$result=Join-Path $root ('TestResults/RimTest-'+$Profile+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$save=Join-Path $result 'SaveData'
$normal=Join-Path $env:USERPROFILE 'AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Config'
$hashes=@{}
foreach($name in @('ModsConfig.xml','Prefs.xml')){$hashes[$name]=(Get-FileHash (Join-Path $normal $name) -Algorithm SHA256).Hash}
$production=Join-Path $root 'Assemblies/AncientMedievalJapanWaterworks.dll'
$dllHash=(Get-FileHash $production -Algorithm SHA256).Hash
$owned=$false
try {
    New-Item -ItemType Directory -Force -Path (Join-Path $fixture 'About'),(Join-Path $fixture 'Assemblies'),(Join-Path $save 'Config') | Out-Null
    $owned=$true
    Set-Content -LiteralPath (Join-Path $fixture '.amjw-test-owned') -Value 'rimtest' -Encoding Ascii
    $about='<ModMetaData><name>Waterworks RimTest (temporary)</name><author>sucRo</author><packageId>sucro.amjw.rimtests</packageId><supportedVersions><li>1.6</li><li>1.7</li></supportedVersions><modDependencies><li><packageId>ilyvion.rimtestredux</packageId><displayName>RimTest Redux</displayName></li></modDependencies></ModMetaData>'
    [IO.File]::WriteAllText((Join-Path $fixture 'About/About.xml'),$about)
    $rimtest=Find-RimWorldModAssembly (Join-Path $workshop '3762405308') 'RimTestRedux.dll' $runtime.Minor
    $harmony=Find-RimWorldModAssembly (Join-Path $workshop '2009463077') '0Harmony.dll' $runtime.Minor
    Invoke-RimWorldTestCompile $RimWorldDir (Join-Path $fixture 'Assemblies/AMJW.Waterworks.RimTests.dll') @((Join-Path $root 'Tests/RimTest/WaterworksRimTests.cs')) @($rimtest,$harmony,$production)
    [xml]$config=Get-Content -LiteralPath (Join-Path $normal 'ModsConfig.xml') -Raw -Encoding UTF8
    $active=$config.SelectSingleNode('/ModsConfigData/activeMods'); $active.RemoveAll()
    $ids=@('brrainz.harmony','ludeon.rimworld','ilyvion.laboratory','ilyvion.rimtestredux')
    if($Profile -ne 'Solo'){$ids+='dubwise.dubsbadhygiene'}
    if($Profile -eq 'Medieval'){$ids+='eldersign.dbhformedieval'}
    $ids+=@('sucro.ancientmedievaljapan.waterworks','sucro.amjw.rimtests')
    foreach($id in $ids){$node=$config.CreateElement('li');$node.InnerText=$id;[void]$active.AppendChild($node)}
    $config.ModsConfigData.version=$runtime.Version.Split(' ')[0]
    $config.Save((Join-Path $save 'Config/ModsConfig.xml'))
    Copy-Item -LiteralPath (Join-Path $normal 'Prefs.xml') -Destination (Join-Path $save 'Config/Prefs.xml')
    $env:AMJW_RIMTEST_RESULT=Join-Path $result 'RimTestSummary.json'
    $log=Join-Path $result 'Player.log'
    Invoke-RimWorldIsolated $RimWorldDir ('-savedatafolder="'+$save+'" -logFile "'+$log+'"') $log 180
    $summary=Get-Content -LiteralPath $env:AMJW_RIMTEST_RESULT -Raw -Encoding UTF8 | ConvertFrom-Json
    if(-not $summary.passed -or $summary.total -ne 4 -or $summary.pass -ne 4 -or $summary.failed -ne 0 -or $summary.skipped -ne 0 -or $summary.unknown -ne 0){throw 'RimTest requires 4/4 without skipped/unknown tests'}
    if([IO.Path]::GetFullPath($summary.productionAssembly) -ne [IO.Path]::GetFullPath($production)){throw 'Wrong production assembly loaded'}
    Write-Output "[OK] RimTest $Profile 4/4, runtime ERROR 0: $result"
} finally {
    Remove-Item Env:AMJW_RIMTEST_RESULT -ErrorAction SilentlyContinue
    if($owned){
        $resolved=[IO.Path]::GetFullPath($fixture)
        if($resolved -ne [IO.Path]::GetFullPath((Join-Path $mods 'AncientMedievalJapanWaterworks.RimTests')) -or (Get-Content -LiteralPath (Join-Path $fixture '.amjw-test-owned') -Raw -Encoding UTF8).Trim() -ne 'rimtest'){throw 'Unsafe RimTest fixture cleanup'}
        Move-Item -LiteralPath $fixture -Destination (Join-Path $result 'RetiredFixture')
    }
    if((Get-FileHash $production -Algorithm SHA256).Hash -ne $dllHash){throw 'Production DLL changed during RimTest'}
    foreach($name in $hashes.Keys){if((Get-FileHash (Join-Path $normal $name) -Algorithm SHA256).Hash -ne $hashes[$name]){throw "Normal $name changed"}}
}
