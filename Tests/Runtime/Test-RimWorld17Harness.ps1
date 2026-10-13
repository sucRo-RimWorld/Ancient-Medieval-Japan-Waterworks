$ErrorActionPreference = 'Stop'
$modulePathBefore = $env:PSModulePath
. (Join-Path $PSScriptRoot '../../Scripts/RimWorldRuntime.ps1')
if ($PSVersionTable.PSEdition -eq 'Core' -and $env:PSModulePath -ne $modulePathBefore) {
    throw 'Runtime helper changed PowerShell Core module paths; clean-session commands cannot autoload.'
}
$temp = Join-Path ([IO.Path]::GetTempPath()) ('AMJW17-' + [guid]::NewGuid().ToString('N'))
function Assert([bool]$ok, [string]$message) { if (-not $ok) { throw $message } }
function MustFail([scriptblock]$action, [string]$message) {
    $failed = $false
    try { & $action | Out-Null } catch { $failed = $true }
    Assert $failed $message
}
try {
    $game = Join-Path $temp 'RimWorld'
    New-Item -ItemType Directory -Force -Path $game | Out-Null
    Set-Content -LiteralPath (Join-Path $game 'Version.txt') -Value '1.7.4976 rev1043' -Encoding Ascii
    [IO.File]::WriteAllBytes((Join-Path $game 'RimWorld.exe'), [byte[]]@())
    $target = Get-RimWorldRuntime $game
    Assert ($target.Modern -and $target.Minor -eq '1.7' -and
        $target.Exe.EndsWith('RimWorld.exe')) '1.7 native executable was not selected'
    Set-Content -LiteralPath (Join-Path $game 'Version.txt') -Value '1.6.4871' -Encoding Ascii
    [IO.File]::WriteAllBytes((Join-Path $game 'RimWorldWin64.exe'), [byte[]]@())
    Assert (-not (Get-RimWorldRuntime $game).Modern) '1.6 compatibility path was changed'
    Set-Content -LiteralPath (Join-Path $game 'Version.txt') -Value '1.8.0' -Encoding Ascii
    MustFail { Get-RimWorldRuntime $game } 'Unsupported engine version was accepted'
    $provider = Join-Path $temp 'Provider'
    foreach ($v in @('1.6', '1.7')) {
        $dir = Join-Path $provider "$v/Assemblies"
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
        [IO.File]::WriteAllBytes((Join-Path $dir 'Quickstarts.dll'), [byte[]]@(1))
    }
    @'
<loadFolders>
  <v1.6><li>1.6</li></v1.6>
  <v1.7><li>/</li><li>1.7</li></v1.7>
</loadFolders>
'@ | Set-Content -LiteralPath (Join-Path $provider 'LoadFolders.xml') -Encoding UTF8
    Assert ((Find-RimWorldModAssembly $provider 'Quickstarts.dll' '1.7') -eq
        [IO.Path]::GetFullPath((Join-Path $provider '1.7/Assemblies/Quickstarts.dll'))) 'Wrong 1.7 DLL selected'
    @'
<loadFolders><v1.6><li>1.6</li></v1.6></loadFolders>
'@ | Set-Content -LiteralPath (Join-Path $provider 'LoadFolders.xml') -Encoding UTF8
    Assert ((Find-RimWorldModAssembly $provider 'Quickstarts.dll' '1.7') -eq
        [IO.Path]::GetFullPath((Join-Path $provider '1.6/Assemblies/Quickstarts.dll'))) '1.6 fallback changed'
    Remove-Item -LiteralPath (Join-Path $provider '1.6/Assemblies/Quickstarts.dll')
    MustFail { Find-RimWorldModAssembly $provider 'Quickstarts.dll' '1.7' } 'Missing declared provider was accepted'
    $log = Join-Path $temp 'Player.log'
    [IO.File]::WriteAllText($log, "Process: pid 123`n2026-10-13 01:02:03.100 INFO [Verse] ERROR count=0`n")
    Assert-RimWorldNativeLog $log 123
    foreach ($text in @("Process: pid 124`n", "Process: pid 123`n2026-10-13 01:02:03.100 ERROR [Verse] failed`n", '')) {
        [IO.File]::WriteAllText($log, $text)
        MustFail { Assert-RimWorldNativeLog $log 123 } 'Wrong or erroneous native log was accepted'
    }
    # UTF-8 metadata and legacy Unity references caused real local failures.
    $runnerSource = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../../Scripts/Run-17Tests.ps1') -Raw -Encoding UTF8
    Assert ($runnerSource.Contains('Get-Content -LiteralPath $source -Raw -Encoding UTF8')) 'UTF-8 About read regressed'
    foreach ($sourceName in @('WaterworksSteps.cs','WaterworksPawnJobs.cs','WaterworksPersistenceSteps.cs','WaterworksMedievalSteps.cs')) {
        Assert ($runnerSource.Contains("'"+$sourceName+"'")) ('Missing native test source: '+$sourceName)
    }
    $steps = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../E2E/WaterworksSteps.cs') -Raw -Encoding UTF8
    Assert ($steps.Contains('"RimWorks.Pickle.Runtime.PickleDriver"') -and $steps.Contains('new[] { typeof(Action) }') -and $steps.Contains('UnityData.IsInMainThread')) 'Legacy dispatcher bridge lost signature/thread safeguards'
    $medievalFeature = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../E2E/TestMod/Pickle/Features/waterworks-medieval.feature') -Raw -Encoding UTF8
    $expectedIntegration = @($medievalFeature -split "`n" | Where-Object { $_ -match '^\s*Scenario:' }).Count
    Assert ($expectedIntegration -eq 6) 'Medieval acceptance must retain all six scenarios including map removal'
    $lifecycleName = 'Separate maps never share supply and native map removal clears the provider'
    foreach ($relative in @('../../Scripts/Run-17Tests.ps1','../../Scripts/run-compatibility-e2e.ps1')) {
        $content = Get-Content -LiteralPath (Join-Path $PSScriptRoot $relative) -Raw -Encoding UTF8
        Assert ($content.Contains($lifecycleName)) 'Native map lifecycle scenario must be mandatory in both version runners'
    }
    Write-Output '[OK] Waterworks 1.6/1.7 runtime selection, provider selection, PID and ERROR contracts passed (no game).'
}
finally {
    if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp -Recurse -Force }
}
