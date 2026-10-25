$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$processFile = Join-Path $repoRoot 'artifacts/runtime/processes.json'
if (-not (Test-Path -LiteralPath $processFile)) { return }
foreach ($entry in @(Get-Content -LiteralPath $processFile -Raw | ConvertFrom-Json)) {
    $processInfo = Get-CimInstance Win32_Process -Filter "ProcessId=$($entry.processId)"
    if (-not $processInfo) { continue }
    $expected = if ($entry.service -eq 'BlazorWasmClient') { 'BlazorWasmClient/BlazorWasmClient.csproj' } else { "$($entry.service).dll" }
    if ($processInfo.Name -eq 'dotnet.exe' -and $processInfo.CommandLine.Contains($expected)) {
        Stop-Process -Id $entry.processId
        Write-Host "Stopped $($entry.service) ($($entry.processId))"
    }
}
