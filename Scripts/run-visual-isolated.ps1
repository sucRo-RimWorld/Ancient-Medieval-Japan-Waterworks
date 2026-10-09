param([switch]$Core)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Add-Type -Path (Join-Path $root 'Tests/E2E/PrivateDesktopNative.cs')
$null = New-Item -ItemType Directory -Force -Path (Join-Path $root 'TestResults')
$batch = Join-Path $root 'TestResults/run-visual-private.cmd'
$log = Join-Path $root $(if ($Core) {'TestResults/core-launcher.log'} else {'TestResults/visual-launcher.log'})
$runner = Join-Path $PSScriptRoot $(if ($Core) {'run-e2e.ps1'} else {'run-visual-e2e.ps1'})
$body = '@echo off' + "`r`n" + 'powershell.exe -NoProfile -ExecutionPolicy Bypass -File "' + $runner + '" > "' + $log + '" 2>&1' + "`r`n" + 'exit /b %errorlevel%'
Set-Content -LiteralPath $batch -Value $body -Encoding ASCII
$code = [ReleaseMatrixDesktopRunner]::Main(@($batch,$root))
if ($code -ne 0) { throw "Visual run failed with exit $code. See $log" }
Write-Output 'Private desktop visual run completed; desktop was never switched.'
