param(
    [ValidateSet('EW')]
    [string]$Mask = 'EW',
    [switch]$All,
    [string]$Python = 'python'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$pipeline = Join-Path $PSScriptRoot 'canal_tile_pipeline.py'
$outDir = Join-Path $root "TestResults/CanalTiles/$Mask"

if ($All) {
    throw '-All is intentionally blocked until the EW baseline receives explicit visual approval.'
}

if (-not (Test-Path $pipeline)) {
    throw "Missing canal pipeline: $pipeline"
}

& $Python $pipeline --mask $Mask --out $outDir
if ($LASTEXITCODE -ne 0) {
    throw "Canal tile pipeline failed with exit code $LASTEXITCODE"
}

$report = Join-Path $outDir 'validation.json'
if (-not (Test-Path $report)) {
    throw "Pipeline returned without validation report: $report"
}

$data = Get-Content $report -Raw | ConvertFrom-Json
if ($data.status -ne 'PASS') {
    throw "Canal candidate did not pass mechanical validation."
}

Write-Host "Candidate exported: $outDir"
Write-Host "Mechanical validation: PASS"
Write-Host "Production approval: false"
